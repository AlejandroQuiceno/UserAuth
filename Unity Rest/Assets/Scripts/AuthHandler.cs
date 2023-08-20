using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.Networking;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.SceneManagement;
using System.Linq;

public class AuthHandler : MonoBehaviour
{
    [SerializeField] TMP_Text userInput;
    [SerializeField] public TMP_Text passwordInput;
    [SerializeField] string url = "https://sid-restapi.onrender.com/api";
    [SerializeField] GameObject userPanel;
    [SerializeField] List<UIuser> UIusers;
    User[] usuarios;
    [SerializeField] UIuser currentUserUI;
    [SerializeField] TMP_Text tokenUI;
    AuthData currentUser;

    public string savedToken;
    public string savedUserName;
    public bool clearPlayerPrefs;
    void Start()
    {
        if(clearPlayerPrefs)
        {
            PlayerPrefs.SetString("token","");
            PlayerPrefs.SetString("username", "");
        }
        savedToken = PlayerPrefs.GetString("token");
        if(savedToken == string.Empty || savedToken == null)
        {
            Debug.Log("No hay un token almacenado");
        }
        else
        {
            savedUserName = PlayerPrefs.GetString ("username");
            StartCoroutine(GetProfile(savedUserName));
            StartCoroutine(GetUsers());
        }
    }
    public void Login()
    {
        AuthData authData = new AuthData();
        authData.username = passwordInput.text;
        authData.password = userInput.text;
        string json = JsonUtility.ToJson(authData);//esto es para contruir el json con la clase modelo
        StartCoroutine(SendLogin(json,name));
    }
    public void Register()
    {
        AuthData authData = new AuthData();
        authData.username = passwordInput.text;
        authData.password = userInput.text;
        string json = JsonUtility.ToJson(authData);//esto es para contruir el json con la clase modelo
        StartCoroutine(SendRegister(json));
    }

    public void AddPoints()
    {
        currentUser.usuario.data.score++;
        User user = new User();
        user.data = new Data();
        Debug.Log("currentUser.username"+ currentUser.username);
        user.username = currentUser.usuario.username;
        user.data.score= currentUser.usuario.data.score;
        currentUserUI.score.text = "Puntaje: " + currentUser.usuario.data.score.ToString();
        string json = JsonUtility.ToJson(user);
        StartCoroutine(PatchUser(json));
    }
    public void SubstractPoints()
    {
        currentUser.usuario.data.score--;
        User user = new User();
        user.data = new Data();
        Debug.Log("currentUser.username" + currentUser.username);
        user.username = currentUser.usuario.username;
        user.data.score = currentUser.usuario.data.score;
        currentUserUI.score.text = "Puntaje: " + currentUser.usuario.data.score.ToString();
        string json = JsonUtility.ToJson(user);
        StartCoroutine(PatchUser(json));
    }
    public void PopulateUsers()
    {
        for(int i =0;i<UIusers.Count;i++)
        {
            UIusers[i].username.text = usuarios[i].username;
            UIusers[i].id.text = usuarios[i]._id;
            UIusers[i].score.text= usuarios[i].data.score.ToString();
        }
    }
    public void populateCurrentUser()
    {
        currentUserUI.username.text = currentUser.username;
        currentUserUI.id.text = "ID: "+currentUser.usuario._id;
        currentUserUI.score.text = "Puntaje: " + currentUser.usuario.data.score.ToString();
        tokenUI.text = currentUser.token;
    }
    IEnumerator GetProfile(string username)
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/usuarios/"+username);//esta ruta se conoce como parametro
        request.SetRequestHeader("x-token", savedToken);     
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log("message" + request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData authData = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                userPanel.SetActive(true);
                currentUser = authData;
                currentUser.usuario.username= username;
                currentUser.token = savedToken;
                Debug.Log(currentUser.token);
                StartCoroutine(GetUsers());
                populateCurrentUser();
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator  GetUsers ()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/usuarios?limit5");//esta ruta se conoce como parametro
        request.SetRequestHeader("x-token", savedToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                UsersArray users = JsonUtility.FromJson<UsersArray>(request.downloadHandler.text);
                foreach(var item in users.usuarios)
                {
                    
                }
                usuarios = users.usuarios;
                usuarios = usuarios.OrderByDescending(user => user.data.score).ToArray();
                PopulateUsers();
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url+"/usuarios",json);//
        request.SetRequestHeader("Content-Type","application/json");
        request.method = "POST";//debo de primero hacerlo con put y luego cambiar el metodo a post porque si lo pongo como solo como put no importa que le ponga el content type en .json, unity lo sobreescribe a texto plano cuando se envia
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("usuario con datos"+data.usuario.username+data.usuario._id);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator SendLogin(string json,string name )
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/auth/login", json);//
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";//debo de primero hacerlo con put y luego cambiar el metodo a post porque si lo pongo como solo como put no importa que le ponga el content type en .json, unity lo sobreescribe a texto plano cuando se envia
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData modelUser = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                PlayerPrefs.SetString("token", modelUser.token);
                PlayerPrefs.SetString("username", modelUser.usuario.username);
                Debug.Log("Se logeo el usuario con" + modelUser.usuario.username +" "+ modelUser.usuario._id + "y token:"+modelUser.token);
                userPanel.SetActive(true);
                currentUser = modelUser;
                savedToken = modelUser.token;
                savedUserName = modelUser.username;
                currentUser.username = modelUser.usuario.username;
                currentUser.token = savedToken;
                StartCoroutine(GetUsers());
                populateCurrentUser();
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator PatchUser(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url+"/usuarios", json);//
        request.SetRequestHeader("x-token",savedToken);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "PATCH";//debo de primero hacerlo con put y luego cambiar el metodo a post porque si lo pongo como solo como put no importa que le ponga el content type en .json, unity lo sobreescribe a texto plano cuando se envia
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Debug.Log("Actualizado!");
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
}
// clase para armar el json del registro

public class AuthData
{
    public string username;
    public string password;
    public string token;
    public User usuario;
}

[System.Serializable]
public class UsersArray
{
    public User[] usuarios;
}
[System.Serializable]
public class User
{
    public string username;
    public string _id;
    public bool estado;
    public Data data;
}

[System.Serializable]
public class Data
{
    public int score;
    //public User[] friends;
    //le puedo agregar cualequier cosa
}
[System.Serializable]
public class UIuser
{
    public TMP_Text username;
    public TMP_Text id;
    public TMP_Text score;
}