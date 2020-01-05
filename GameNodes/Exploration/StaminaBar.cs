using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeNotes_Visual {
    
    public class StaminaBar : MonoBehaviour{

        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI staminaCount;

        [Range(0,1)]
        public float staminaLine = 0.5f;

        public int staminaPoints;

        // Start is called before the first frame update
        void Reset()
        {
            _image = GetComponent<Image>();
        }



        // Update is called once per frame
        void Update()
        {
            float thickness =Mathf.Pow(staminaLine, 8) * 0.8 + 0.2 * off.x;
            //float splits = max(0, 0.1 * thickness - 1 + abs(((thickness * 10) % 1) - 0.5) * 2) * 10;


        }
    }
}