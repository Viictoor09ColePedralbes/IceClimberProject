using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveJSON
{
    public static void SaveData()
    {
        PlayerValuesSerializable datosSerializable = new PlayerValuesSerializable();

        string json = JsonUtility.ToJson(datosSerializable, true);
        string ruta = Application.persistentDataPath + "/datosJugador.json";

        File.WriteAllText(ruta, json);

        Debug.Log("Datos guardados en: " + ruta);
    }
}
