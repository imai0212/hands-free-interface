using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.IO;



public class FittsLaw : MonoBehaviour
{
    private Vector3 startPos;       // 1試行の開始位置
    private float startTime;        // 1試行の開始時刻
    private float responseTime, distance, startExperiment;
    public static float experimentTime;    // 1セットの経過時間
    private int numOfSuccess, numOfError;  // 1試行のカウント, 1セット内のエラー回数
    public Text count;
    bool isBreath = false;
    // sphereの識別
    GameObject cursor, startSphere, target, selectSphere, preSphere;
    int intSelectIndex, targetIndex, preIndex;
    string strSelectIndex, selectName, targetName;
    // サウンド
    public AudioClip sound;
    AudioSource audioSource;
    // UDP通信関係
    int LOCAL_PORT = 22222;  //受信に使うポートの番号
    static UdpClient udp;    //UDPを使う準備
    private static short[] s_buf = new short[1];  //データ格納用のshort型変数（２バイト)
    // csv出力するデータの格納用
    List<string> breathList = new List<string>();
    List<string> fittsData = new List<string>();


    void Start()
    {
        numOfSuccess = 0;
        cursor = GameObject.Find("Cursor");
        // 入力を開始するsphere
        selectSphere = GameObject.Find("1(Clone)");
        selectSphere.GetComponent<Renderer> ().material.color = new Color(0f, 1f, 0f, 1f);
        selectName = "1(Clone)";
        // 最初のターゲット
        target = GameObject.Find("7(Clone)");
        targetName = "7(Clone)";
        target.GetComponent<Renderer> ().material.color = new Color(1f, 0f, 0f, 1f);
        // サウンド
        audioSource = GetComponent<AudioSource>();
        // UDP通信
        udp = new UdpClient(LOCAL_PORT);
        // csv出力のカラム
        fittsData.Add("responseTime,distance");
    }

    // EndFittsLawスクリプトから呼び出す関数
    public static float SendExperimentTime()
    {
        return experimentTime;
    }

    void Update()
    {
        // rayの設定
        OVRCameraRig cameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        Ray ray = new Ray(cameraRig.centerEyeAnchor.position, cameraRig.centerEyeAnchor.forward);
        RaycastHit hit;
        // 受信の準備
        IPEndPoint remoteEP = null;
        byte[] data = udp.Receive(ref remoteEP);
        //パケットサイズをチェックし、受信データをUnityに反映
        if ( data.Length == 2 )
        {
            s_buf[0] = BitConverter.ToInt16(data, 0);
            if (s_buf[0] == 0)
            {
                cursor.GetComponent<Renderer> ().material.color = new Color(1f, 1f, 1f, 0.8f);
                breathList.Add($"{DateTime.Now},0");
            }
            else
            {
                cursor.GetComponent<Renderer> ().material.color = new Color(0f, 100f, 255f, 0.8f);
                breathList.Add($"{DateTime.Now},1");
            }
            // 入力開始
            if ( s_buf[0] == 1 && Physics.Raycast(ray, out hit) && hit.collider.name == selectName && isBreath == false)
            {
                // 実験の開始時間を出力
                if (selectName == "1(Clone)")
                {
                    startExperiment = Time.time;
                }
                startTime = Time.time;
                startPos = hit.point;
                // 現在洗濯中のsphere(照合用) 
                selectName = hit.collider.gameObject.name;
                // 入力開始のsphereを薄くする
                selectSphere = hit.collider.gameObject;
                selectSphere.GetComponent<Renderer> ().material.color = new Color(0f, 1f, 0f, 0.5f);
                strSelectIndex = selectName.Replace("(Clone)", "");
                // ターゲット更新 (現在は入力開始時のsphereがターゲット)
                intSelectIndex = int.Parse(strSelectIndex);
                targetIndex = intSelectIndex + 6;
                if (targetIndex > 13) 
                {
                    targetIndex -= 13;
                }
                target = GameObject.Find($"{targetIndex}(Clone)");
                targetName = target.name;
                // 選択したsphereを薄緑にする
                selectSphere.GetComponent<Renderer> ().material.color = new Color(0f, 1f, 0f, 0.5f);
                isBreath = true;
            }
            // ターゲット到達
            else if ( s_buf[0] == 1 && Physics.Raycast(ray, out hit) && hit.collider.name == targetName && isBreath == true)
            {
                // 到達したターゲットを薄い赤色にする
                target.GetComponent<Renderer> ().material.color = new Color(1f, 0f, 0f, 0.5f);
            }
            // 入力終了
            else if (s_buf[0] == 0 && Physics.Raycast(ray, out hit) && hit.collider.name == targetName && isBreath == true)
            {
                // // 反応時間を求める
                responseTime = Time.time - startTime;
                // 移動距離を計算
                Vector3 endPos = hit.point;
                distance = Vector3.Distance(startPos, endPos);
                // responseTimeList.Add(responseTime);
                // distanceList.Add(distance);
                fittsData.Add($"{responseTime},{distance}");

                selectName = hit.collider.gameObject.name;
                selectSphere = hit.collider.gameObject;
                // サウンド再生
                audioSource.PlayOneShot(sound);
                strSelectIndex = selectName.Replace("(Clone)", "");
                intSelectIndex = int.Parse(strSelectIndex);
                targetIndex = intSelectIndex + 6;
                if (targetIndex > 13)
                {
                    targetIndex -= 13;
                }
                // 選択中のsphereがターゲットなら色を緑にする
                if (selectName == $"{strSelectIndex}(Clone)")
                {
                    // 選択したsphereを緑色にする
                    selectSphere.GetComponent<Renderer> ().material.color = new Color(0f, 1f, 0f, 1f);
                }
                target = GameObject.Find($"{targetIndex}(Clone)");
                // ターゲットの色を赤色にする
                target.GetComponent<Renderer> ().material.color = new Color(1f, 0f, 0f, 1f);
                targetName = target.name;
                // 一個前のsphereをデフォルトに戻す
                preIndex = intSelectIndex - 6;        
                if (preIndex < 1)
                {
                    preIndex += 13;
                }
                preSphere = GameObject.Find($"{preIndex}(Clone)");
                preSphere.GetComponent<Renderer> ().material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                isBreath = false;
                // 成功回数のカウント
                numOfSuccess += 1;
                count.text = $"Count : {numOfSuccess}";
                if (numOfSuccess == 13)
                {
                    experimentTime = Time.time - startExperiment;
                    string pathA = Application.persistentDataPath + "/fitts.csv";
                    string pathB = Application.persistentDataPath + "/breath.csv";
                    File.WriteAllLines(pathA, fittsData);
                    File.WriteAllLines(pathB, breathList);
                    SceneManager.LoadScene("EndFittsLaw");
                }
            }
        }
    }
}