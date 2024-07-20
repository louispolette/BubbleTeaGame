using UnityEngine;

public class Speen : MonoBehaviour
{
    public void Spin(float spinAmount)
    {
        transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + spinAmount * Time.deltaTime);
    }
}
