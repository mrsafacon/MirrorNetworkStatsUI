using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NetworkStatsUI : MonoBehaviour {
    public static NetworkStatsUI instance;

    public static void Toggle(bool enabled){
        if(instance != null) instance.enabled = enabled;
    }

    Text text; //optional UI element
    string data;

    List<int> recentIn = new List<int>();
    int thisSecondIn = 0;

    List<int> recentOut = new List<int>();
    int thisSecondOut = 0;

    float nextUpdate;

    void Awake() {
        text = GetComponent<Text>();
        instance = this;
    }

    void Start() {
        //set up delegates
        NetworkDiagnostics.InMessageEvent += input;
        NetworkDiagnostics.OutMessageEvent += output;
    }

    void OnDestroy() {
        NetworkDiagnostics.InMessageEvent -= input;
        NetworkDiagnostics.OutMessageEvent -= output;
    }

    void Update() {
        if(!Mirror.NetworkServer.active && !Mirror.NetworkClient.active){
            data = "Network Not Connected.";
            if(text != null) text.text = data;
        }

        if (nextUpdate < Time.realtimeSinceStartup) {
            //add to lists
            recentIn.Add(thisSecondIn);
            recentOut.Add(thisSecondOut);

            //only need the last few seconds
            if (recentIn.Count > 4) recentIn.RemoveAt(0);
            if (recentOut.Count > 4) recentOut.RemoveAt(0);

            //average them out
            float inFloat = 0;
            foreach (int i in recentIn) inFloat += i;
            inFloat = inFloat / (float)recentIn.Count;

            float outFloat = 0;
            foreach (int i in recentOut) outFloat += i;
            outFloat = outFloat / (float)recentOut.Count;

            //convert to kBps
            inFloat = inFloat / 1000f;
            outFloat = outFloat / 1000f;

            //format to readable string
            data = "kBps in: " +  (inFloat.ToString("n2")) + System.Environment.NewLine;
            data += "kBps out: " + (outFloat.ToString("n2")) + System.Environment.NewLine;

            thisSecondIn = 0;
            thisSecondOut = 0;

            if (NetworkServer.active) {
                //server shows quantity of connected clients
                data += "connections: " + NetworkServer.connections.Count;
            } else if (NetworkClient.isConnected) {
                //client shows latency
                string s = Mathf.RoundToInt(((float)NetworkTime.rtt) * 1000).ToString();
                data += "latency: " + s + System.Environment.NewLine;
            }

            //set UI
            if(text != null) text.text = data;

            nextUpdate = Time.realtimeSinceStartup + 1f;
        }
    }

    void OnDisable(){
        data = "";
        if(text != null) text.text = "";
    }

    public void input(NetworkDiagnostics.MessageInfo info) {
        thisSecondIn += info.bytes;
    }

    public void output(NetworkDiagnostics.MessageInfo info) {
        thisSecondOut += info.bytes * info.count;
    }

    public override string ToString() {
        return data;
    }
}
