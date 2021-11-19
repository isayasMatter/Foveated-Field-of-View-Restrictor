using UnityEngine;
using System.Collections.Generic;



public class GazeTrackerVive : MonoBehaviour
{        
    public static float gaze_x;
	public static float gaze_y;
    
        
    private List<float> gazeArrayX = new List<float>();
    float gazeAverageX = 0.0F;

    private List<float> gazeArrayY = new List<float>();
    float gazeAverageY = 0.0f;

    // The size of the sliding window for the runnoing gaze smoother
	public float smoothingWindow = 10;

    //Initialization done once:
    float lowPassFactor = 0.8f; //Value should be between 0.01f and 0.99f. Smaller value is more damping.
    bool init = true;

    //Buffer for the intermediate value of the low pass filter
    Vector2 intermediateValueBuf;


    public float smoothTime = 0.15f;
    private float _avSlewX;
    private float _avSlewY;

    private float _tunnel_x;
    private float _tunnel_y;

    private void Start()
    {        
              
       
    }
    
    private void Update()
    {
        // Replace this with your gaze data in normalized coordinates.
        Vector2 normalizedGaze = new Vector2(0.5f,0.5f);

        // Filter the raw signal to remove jitter. You can implement your won filter or use the low pass filter implemented here
        Vector2 filteredGaze = lowPassFilter(normalizedGaze, ref intermediateValueBuf, lowPassFactor, true);

        // Smooth the signal with running average smoothing or smooth damping to remove sudden spikes.
        Vector2 smoothedGaze = 
          
        Sigtrap.ImageEffects.TunnellingDynamic.offsetx = smoothedGaze.x;
		Sigtrap.ImageEffects.TunnellingDynamic.offsety = smoothedGaze.y;
            

            
    }


    Vector2 lowPassFilter(Vector2 targetValue, ref Vector2 intermediateValueBuf, float factor, bool init)
    {

        Vector2 intermediateValue;

        //intermediateValue needs to be initialized at the first usage.
        if (init)
        {
            intermediateValueBuf = targetValue;
        }

        intermediateValue.x = (targetValue.x * factor) + (intermediateValueBuf.x * (1.0f - factor));
        intermediateValue.y = (targetValue.y * factor) + (intermediateValueBuf.y * (1.0f - factor));       

        intermediateValueBuf = intermediateValue;

        return intermediateValue;
    }

    void RunningAverageFilter(float x, float y){

        gazeAverageY = 0.0f;
        gazeAverageX = 0.0f;

        gazeArrayY.Add(y);
        gazeArrayX.Add(x);

        if (gazeArrayY.Count >= smoothingWindow) {
            gazeArrayY.RemoveAt(0);
        }
        if (gazeArrayX.Count >= smoothingWindow) {
            gazeArrayX.RemoveAt(0);
        }

        for(int j = 0; j < gazeArrayY.Count; j++) {
            gazeAverageY += gazeArrayY[j];
        }
            
        for(int i = 0; i < gazeArrayX.Count; i++) {
            gazeAverageX += gazeArrayX[i];
        }

        gazeAverageY /= gazeArrayY.Count;
        gazeAverageX /= gazeArrayX.Count;
        		    	
	}



    void SpringDampingFilter(float x, float y)
    {
        _tunnel_x = Mathf.SmoothDamp(_tunnel_x, x, ref _avSlewX, smoothTime);
        _tunnel_y = Mathf.SmoothDamp(_tunnel_y, y, ref _avSlewY, smoothTime);
            
    }
}
