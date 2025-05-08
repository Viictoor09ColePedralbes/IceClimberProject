using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MongoDB_Script : MonoBehaviour
{
    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> usersCollection;

    private const string connectionString = "mongodb+srv://admin:admin@iceclimberdb.6t8buo1.mongodb.net/?retryWrites=true&w=majority&appName=IceClimberDB";

    private static MongoDB_Script mongoScript;
    public static MongoDB_Script instance
    {
        get
        {
            return RequestMongoDB_Script();
        }
    }

    void Awake()
    {
        RequestMongoDB_Script();
    }

    async void Start()
    {
        try
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase("IceClimberDB");
            usersCollection = database.GetCollection<BsonDocument>("UserTelemetry");

            Debug.Log("Conexión a MongoDB realizada correctamente");
        }
        catch (System.Exception e) 
        {
            Debug.LogError("MongoDB Connection Error: " + e.Message);
        }
    }

    public async void InsertData() // Función para insertar datos en la base de datos
    {
        BsonDocument bsonDoc = GameManager.instance.CreateUserTelemetryBSON();
        try
        {
            await usersCollection.InsertOneAsync(bsonDoc);
            Debug.Log("Data insertada correctamente");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cannot insert bsonDoc into Database: " + e.Message);
        }
    }

    private static MongoDB_Script RequestMongoDB_Script()
    {
        if (!mongoScript)
        {
            mongoScript = FindObjectOfType<MongoDB_Script>();
            DontDestroyOnLoad(mongoScript);
        }
        return mongoScript;
    }
}
