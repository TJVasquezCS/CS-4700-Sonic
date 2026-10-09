using UnityEngine;

public class UI : MonoBehaviour
{
    public SonicController sonicController;

    public TMPro.TextMeshProUGUI ringsText;

    private void Update()
    {
        //Update the text to show the current number of rings
        ringsText.text = "Rings: " + sonicController.rings.ToString();
    }
}
