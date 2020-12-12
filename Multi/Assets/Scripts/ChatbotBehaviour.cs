using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ChatbotBehaviour : MonoBehaviour {

    public List<int[]> chatroomBotIndex = new List<int[]>();
    public List<ChatbotAI> chatbotAIs = new List<ChatbotAI>();

    public int nextSessionID;

    protected string text, response;
    protected string sessionId; // Remains null until after first message is parsed
    protected bool waiting;

    public string botid = "rabidrosie";// Name of chatbot to use. Has to be made using website
    public string appid = "una2165008"; // From Pandorabots application
    public string userkey = "2a901bdef12f158b9b6e9bd277d04766";//From Pandorabots application

    public List<Response> responses = new List<Response>();

    public NetworkManagerAT networkManager;

    public string getResponse() {
        return response;
    }

    void Start() {
        text = "";
        response = "Waiting for text";
        DontDestroyOnLoad(gameObject);
    }

    public void GameStart() {
        InitializeChatroomBotIndex();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Chatbot Management

    private void InitializeChatroomBotIndex() {
        for (int i = 0; i < networkManager.nrChatrooms; i++) {
            int[] newArr = { -1, -1 };
            chatroomBotIndex.Add(newArr);
        }
    }

    public void ChangeChatroomBotIndex(int chatroomID, int chatbotID, bool left) {
      //  Debug.Log("change chatroom bot index: " + chatroomID + ", " + chatbotID + ", " + left);
        int i = 0;
        if (!left) i = 1;
        chatroomBotIndex[chatroomID][i] = chatbotID;
        foreach (ChatbotAI c in chatbotAIs) //ebug.Log(c.fakeName);
            foreach (int[] arr in chatroomBotIndex) ;//Debug.Log(arr[0] + ", " + arr[1]);
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Pandora Bot API

    string sanitizePandoraResponse(string wwwText) {
        string responseString = "";

        int startIndex = wwwText.IndexOf(" [") + 2;
        int endIndex = wwwText.IndexOf("],");
        responseString = wwwText.Substring(startIndex, endIndex - startIndex);

        //Debug.Log("Sanitized response: " + responseString);
        return responseString;
    }

    //void getSessionIdOfPandoraResponse(string wwwText) {
    //    int startIndex = wwwText.IndexOf("sessionid") + 12;
    //    int endIndex = wwwText.IndexOf("}") - 1;

    //    sessionId = wwwText.Substring(startIndex, endIndex - startIndex);
    //}

    private IEnumerator PandoraBotRequestCoRoutine(string text, int chatroomID, int sessionID, int chatbotID) {

        string url = "https://api.pandorabots.com/talk?botkey=RssstjtodsmGn5b1IstcJtNZI9khFR8B6xS0_Qvmtrrq5dalb0KYSIeonmRa15PUOL2I-8EtsPdp9rI_1dsWOQ~~&input=";
        url += UnityWebRequest.EscapeURL(text);
        //url += "sessionid=" + sessionID;
        //session id gives weird results somehow
        
        UnityWebRequest wr = UnityWebRequest.Post(url, ""); //You cannot do POST with empty post data, new byte is just dummy data to solve this problem

        yield return wr.SendWebRequest();

        if (wr.error == null) {
            //getSessionIdOfPandoraResponse(wr.downloadHandler.text);

            string r = sanitizePandoraResponse(wr.downloadHandler.text); //Where we get our chatbots response message
            Debug.Log(r);
            r =   r.Remove(0,1);
            r  = r .Remove(r.Length - 1, 1);
            Debug.Log(r);
            Response response = new Response(chatroomID, r, chatbotAIs[chatbotID].fakeName, chatbotAIs[chatbotID].playerVisualPalletID);
            responses.Add(response);
            SendResponseToServer(response);
        }
        else {
            Debug.LogWarning(wr.error);
        }
    }

    public struct Response {
        public Response(int id, string t, string n, int visID) {
            chatroomID = id;
            text = t;
            fakeName = n;
            visualPalletID = visID;
        }
        public int chatroomID;
        public string text;
        public string fakeName;
        public int visualPalletID;
    }

    public void SendTextToChatbot(string text, int chatroomID, int chatbotID) {
        Debug.Log("ChatbotBehaviour, SendTextToChatbot, chatroom id = " + chatroomID);
        Debug.Log("chatbotbehaviour, sendtexttochatbot, chatbot name = " + chatbotAIs[chatbotID].fakeName);
        int sessionID = chatbotAIs[chatbotID].currentSessionID;
        StartCoroutine(PandoraBotRequestCoRoutine(text, chatroomID, sessionID, chatbotID));
    }

    public void SendResponseToServer(Response response) {
        Debug.Log("ChatbotBehaviour, SendResponseToServer, chatroom id = " + response.chatroomID + ", message = " + response.text + "name = " + response.fakeName);
        //networkManager.GamePlayers[0].ReceiveMessageFromChatbot(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
        //networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
        StartCoroutine(WaitToSendResponse(response));
    }

    IEnumerator WaitToSendResponse(Response response) {
        float waitTime;
        string message = response.text;
        waitTime = 0.1f * response.text.Length + response.text.Length * Random.Range(0f, 0.1f);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("send message from chatbot, message = " + message);
        networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(message, response.chatroomID, response.fakeName, response.visualPalletID);
    }

    public void SendResponseToServerDirectly(Response response) {
        networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
    }
}
