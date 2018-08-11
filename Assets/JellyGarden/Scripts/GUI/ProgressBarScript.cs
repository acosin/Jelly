using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProgressBarScript : MonoBehaviour {
	Image slider;
	public static ProgressBarScript Instance;
	float maxWidth;
	public GameObject[] stars;
	// Use this for initialization
	void OnEnable () {
		Instance = this;
		slider = GetComponent<Image> ();
		maxWidth = 1;
		InitBar ();//2.1.2
	}

	public void InitBar () {//2.1.2
		ResetBar ();
	}

	public void UpdateDisplay (float x) {
		slider.fillAmount = maxWidth * x;
		if (maxWidth * x >= maxWidth) {
			slider.fillAmount = maxWidth;

			//	ResetBar();
		}
	}

	public void AddValue (float x) {
		UpdateDisplay (slider.fillAmount * 100 / maxWidth / 100 + x);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool IsFull () {
		if (slider.fillAmount >= maxWidth) { 
			ResetBar ();
			return true;
		} else
			return false;
	}

	public void ResetBar () {
		UpdateDisplay (0.0f);
	}


}
