using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpTest : MonoBehaviour
{
    public string usersUrl= "https://my-json-server.typicode.com/AlejandroQuiceno/jsonDB";//host o url base
    public string charactersUrl = "https://rickandmortyapi.com/api/character";

    UserList_Model users;
    [SerializeField] List<RawImage> images = new List<RawImage>();
    [SerializeField] int currentUser;

    //
    [SerializeField] TMP_Text Name;
    [SerializeField] TMP_Text ID;
    private void Start()
    {
        UpdateUser();
    }
    private void UpdateUserText()
    {
        Name.text =users.ListUsers[currentUser].name;
        ID.text = "ID "+users.ListUsers[currentUser].id.ToString();
    }
    private void UpdateUser()
    {
        StartCoroutine(GetUsers(usersUrl));
    }
    private void Getcharacters()
    {
        for (int i = 0; i < users.ListUsers[currentUser].deck.Length; i++)
        {
            StartCoroutine(GetCharacter(charactersUrl, users.ListUsers[currentUser].deck[i],i));
        }
    }
    public void NextUser()
    {
        if(currentUser + 1< users.ListUsers.Count)
        {
            currentUser++;
            UpdateUser();
        }
    }
    public void PrevUser()
    {
        if (currentUser - 1 >= 0)
        {
            currentUser--;
            UpdateUser();
        }
    }
    IEnumerator GetUser()
    {
        UnityWebRequest request  = UnityWebRequest.Get(usersUrl + "/users"+1);//porque puedo tener mas end points
        yield return request.SendWebRequest();
        if (request.isNetworkError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);//Me da el mensaje de la peticion
            if (request.responseCode == 200)// esa es la respuesta de la api
            {
                // se empieza la tranformacion de la data
                //esta viene en un string
                //debemos conocer la data y conocer su estructura y crear clase modelo
                User_model user = JsonUtility.FromJson<User_model>(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator GetUsers(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/users");//porque puedo tener mas end points
        yield return request.SendWebRequest();
        if (request.isNetworkError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);//Me da el mensaje de la peticion
            if (request.responseCode == 200)// esa es la respuesta de la api
            {
                // se empieza la tranformacion de la data
                //esta viene en un string
                //debemos conocer la data y conocer su estructura y crear clase modelo
                users = JsonUtility.FromJson<UserList_Model>(request.downloadHandler.text);
                UpdateUserText();
                Getcharacters();
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    IEnumerator GetCharacter(string url,int characterNumber,int index)
    {
        UnityWebRequest request = UnityWebRequest.Get(url+"/"+characterNumber);//porque puedo tener mas end points
        yield return request.SendWebRequest();
        if (request.isNetworkError)//primero miro si hay un error de red
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Character_model character = JsonUtility.FromJson<Character_model>(request.downloadHandler.text);
            StartCoroutine(DownloadImage(character.image, index));
        }
    }
    IEnumerator DownloadImage(string mediaUrl,int index)
    {
 
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                images[index].texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
        }
    }

    [System.Serializable]
    class UserList_Model
    {
        public List<User_model> ListUsers; // tengo que arreglar el json para que tenga un objeto llamado
                                // users dentro de el endpoint de users para que unity pueda
                                // interpertar esa clase modelo
                                // le tengo que decir a json utility que dento de la respuestas hay una lista de usuarios que se llama users 
                                // en otras palabras hay que hacer una clave dentro de otra clave
    }
    [System.Serializable] // hay que hacer que estos objetos sean serializables para poder 
     class User_model //clase modelo
     {
        public string name;
        public int id;
        public int[] deck;
        public override string ToString()
        {
            string response = "Usuario con el id de:" + id + "y el nombre de:" + name+", con un estado de: "+deck[2];
            return response;
        }
     }
    [System.Serializable]
    class Character_model
    {
        public string name;
        public string image;
        public override string ToString()
        {
            string response = "personaje con el nombre de:" + name + "y la url de:"+image;
            return response;
        }
    }
}
