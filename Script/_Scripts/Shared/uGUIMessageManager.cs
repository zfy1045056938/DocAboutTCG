using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Message {
	public Text text;
	public float fadeTimer;
	public Color color;
}

public class uGUIMessageManager : MonoBehaviour {

	private List<Message> messages;

	public float fadeTime;

	public float upSpeed;

	public Text prefab;
	
	// Use this for initialization
	void Start () {
		messages = new List<Message>();
	}
	
	// Update is called once per frame
	void Update () {
		//Make the messages float upwards
		if(messages.Count > 0) {
			for(int i = 0; i < messages.Count; i++) {
				messages[i].fadeTimer += Time.deltaTime;
				//If the messages is completely faded then destroy them
				if(messages[i].fadeTimer > fadeTime) {
					Destroy(messages[i].text.gameObject);
					messages.Remove(messages[i]);
					return;
				}
				//Fade the color of the message and make the move upwards
				messages[i].text.rectTransform.localPosition += new Vector3(0,Time.deltaTime * upSpeed,0);
				messages[i].text.color = Color.Lerp(messages[i].color, Color.clear, messages[i].fadeTimer/fadeTime);
			}
		}
	}

	//Instantiate the message and add it to the list
	public void AddMessage(Color color, string message) {
		Message newMessage = new Message();
		newMessage.text = Instantiate(prefab,  transform.position, Quaternion.identity) as Text;
		newMessage.text.transform.SetParent(transform);
		newMessage.text.rectTransform.localScale = Vector3.one;
		newMessage.text.text = message;
		newMessage.color = newMessage.text.color = color;
		messages.Add(newMessage);
	}
}
