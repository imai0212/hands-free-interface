#include <M5Stack.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <Adafruit_MLX90614.h>

const char* ssid = "Buffalo-88C0";//WiFiアクセスポイントのSSID
const char* password = "i4dvkr56p36bu";//WiFiアクセスポイントのパスワード
//const char* ssid = "misaki-lab-g";
//const char* password = "0000000142";
const char* client_address = "192.168.11.11";//送信相手のPCのIP
const int client_port = 22222;  //送り先のポート番号
const int server_port = 22224;  //ESP32のポート番号
WiFiUDP udp;
// 呼気関係
double temp = 0.0;    // 現在温度
double tempMax = 0.0; // 最大温度
double preTemp = 0.0; // 前回温度
int breath = 0;
Adafruit_MLX90614 mlx = Adafruit_MLX90614();
// バッテリー関係
boolean goon = true;

//共用体の設定(データをショートで格納し、バイト型として取り出せる)
typedef union {
  short sval[1];//ショート型
  uint8_t bval[2];//符号なしバイト型
} UDPData;
UDPData s_upd_message_buf; //共用体のインスタンスを宣言


void setup() {
  M5.begin();
  M5.Power.begin();
  M5.Lcd.setTextSize(2);
  //バッテリー
  if(!M5.Power.canControl()) {
    M5.Lcd.print("can't control Batt.");
    goon = false;
  }
  if (goon) {
    int BatteryLevel = M5.Power.getBatteryLevel();
    M5.Lcd.setCursor(0, 0);
    M5.Lcd.print("Battery Level : ");
    M5.Lcd.printf("%3d %%",BatteryLevel);
    M5.Lcd.println("");
    M5.Lcd.println("");
  }
  //WiFi 初期化
  WiFi.disconnect(true, true);//WiFi接続をリセット
  M5.Lcd.print("Connecting WiFi : " );
  M5.Lcd.println(String(ssid)); //接続先を表示
  delay(100);
  WiFi.begin(ssid, password);//Wifiに接続
  //接続が完了するまでループで待機
  while ( WiFi.status() != WL_CONNECTED) {
    delay(100);
  }
  M5.Lcd.println("WiFi connected.");//WiFi接続完了通知
  M5.Lcd.println("");
  M5.Lcd.println("Temperature Test");
  //UDP 開始
  udp.begin(server_port);
  delay(500);
  // 赤外線センサ
  mlx.begin();
  Serial.begin(115200);
  delay(500);
}

void getData(){
  M5.update();
  tempMax = 0.0;
  unsigned long time = millis();
  M5.Lcd.println("Setup start");
  // 基準温度設定
  while (millis() < time + 20000){
    temp = mlx.readObjectTempC();
    if (tempMax < temp){
      tempMax = temp;
    }
  }
  // 呼気認識
  temp = 0.0;
  M5.Lcd.println("Setup done");
  while (true){
    M5.update();
    // 現在温度を監視
    temp = mlx.readObjectTempC();
    if (temp > tempMax){
      breath = 1;
    }else{
      breath = 0;
    }
    
    //送信するデータを作成
    s_upd_message_buf.sval[0] = (short)(breath);
    udp.beginPacket(client_address, client_port);//UDPパケットの開始
    for (int i = 0; i < 2; i++) {
      udp.write(s_upd_message_buf.bval[i]);//１バイトずつ送信
    }
    udp.endPacket();//UDPパケットの終了
    
    // 前回温度を監視
    preTemp = temp;
    if (M5.BtnA.wasPressed()){
      M5.Lcd.println("reset!");
      break;
    }
  }
}

void loop() {
  M5.update();
  if (M5.BtnB.wasPressed()){
    getData();
  }
}
