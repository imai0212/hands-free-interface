using UnityEngine;

public class SphereCreator : MonoBehaviour
{
    public GameObject spherePrefab; // sphereのプレハブ
    private int numberOfSpheres = 13; // 生成するsphereの個数
    private float radius = 10f; // 円周上に配置するsphereの半径
    int i;

    void Start()
    {
        // 円周上にsphereをnumberOfSpheres個生成する
        for (i = 1; i < numberOfSpheres + 1; i++)
        {
            spherePrefab.name = i.ToString();
            // 配置するsphereの角度を求める (１個あたりの角度に配置するsphereのインデックス番号をかける)
            float angle = 360f / numberOfSpheres * i;
            // sphereを生成する
            GameObject sphere = Instantiate(spherePrefab);
            // sphereを円周上に配置する
            sphere.transform.position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                0f);
            // 生成するsphereの色を指定する
            sphere.GetComponent<Renderer> ().material.color = new Color(
                0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
}
