using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;

public class SaveSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UploadTest()
    {
        StartCoroutine(Upload());
    }

    public IEnumerator Upload()
    {
        using (UnityWebRequest www = new UnityWebRequest("http://127.0.0.1:5000/UnityTest", "POST"))
        {
            JSONObject jo = new JSONObject();
            jo.Add("Test", "Hotkang");

            byte[] payload = Encoding.ASCII.GetBytes(jo.ToString());
            Debug.Log(jo.ToString());
            UploadHandler uploader = new UploadHandlerRaw(payload);
            uploader.contentType = "application/json";

            www.uploadHandler = uploader;
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }
    }
}
