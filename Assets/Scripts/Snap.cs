using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public Camera snapCam;
    int w = 300, h = 300;
    int blockid;

    World world;

    public void setupsnap(World w)
    {
        world = w;
    }

    public void snap()
    {
        snapCam.gameObject.SetActive(true);

        if (snapCam.targetTexture == null)
        {
            snapCam.targetTexture = new RenderTexture(w, h, 24);
        }
        else
        {
            w = snapCam.targetTexture.width;
            h = snapCam.targetTexture.height;
        }
    }

    void LateUpdate()
    {
        if (snapCam.gameObject.activeInHierarchy)
        {
            Texture2D snapshot = new Texture2D(w, h, TextureFormat.ARGB32, false);
            snapCam.Render();
            RenderTexture.active = snapCam.targetTexture;
            snapshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            snapshot.alphaIsTransparency = true;
            Color[] rawdata = snapshot.GetPixels();
            for (int i = 0; i < rawdata.Length; i++)
            {
                if (rawdata[i] == Color.black)
                {
                    rawdata[0] = new Color(0, 0, 0, 0);
                }
            }

            snapshot.SetPixels(rawdata);

            byte[] bytes = snapshot.EncodeToPNG();
            string dataname = world.bds[blockid].Name.ToLower();
            dataname = dataname.Replace(" ","_");
            dataname += ".png";
            string fileName = Application.dataPath + "/Icon/" + dataname;
            System.IO.File.WriteAllBytes(fileName, bytes);
            Debug.Log("[Screenshot] " + fileName);
            blockid++;
            snapCam.gameObject.SetActive(false);
        }
    }
}
