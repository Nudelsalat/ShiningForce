using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour {

    void Awake() {
        var objectName = gameObject.name;
        var sameObject = GameObject.Find(objectName);
        if (sameObject != null && !sameObject.Equals(gameObject)) {
            Debug.Log("Destroying: " + objectName);
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
        }
    }
}

