using UnityEngine;

public class WaponSway : MonoBehaviour
{
    private Quaternion startRotation;
    public float swayAmount = 8;
    void Start()
    {
        startRotation = transform.localRotation;
        
    }

    void Update()
    {
        Sway();  
    }

    private void Sway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Quaternion xAngle = Quaternion.AngleAxis(mouseX * -1.25f, Vector3.up);
        Quaternion yAngle = Quaternion.AngleAxis(mouseY * 1.25f, Vector3.left);
        Quaternion targetRotation = startRotation * xAngle * yAngle;
        transform.localRotation = Quaternion.Lerp(transform.localRotation,targetRotation, Time.deltaTime * swayAmount);
    }
}
