using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
#if MOBILE_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif
using System.Reflection;

namespace Invector.CharacterController
{
    public class vInput : MonoBehaviour
    {
        public delegate void OnChangeInputType(InputDevice type);
        public event OnChangeInputType onChangeInputType;
        private static vInput _instance;
        public static vInput instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<vInput>();
                    if (_instance == null)
                    {
                        new GameObject("vInputType", typeof(vInput));
                        return vInput.instance;
                    }
                }
                return _instance;
            }
        }

        public vHUDController hud;

        void Start()
        {
            if (hud == null) hud = vHUDController.instance;

        }

        private InputDevice _inputType = InputDevice.MouseKeyboard;
        [HideInInspector]
        public InputDevice inputDevice
        {
            get { return _inputType; }
            set
            {
                _inputType = value;
                OnChangeInput();
            }
        }

        /// <summary>
        /// GAMEPAD VIBRATION - call this method to use vibration on the gamepad
        /// </summary>
        /// <param name="vibTime">duration of the vibration</param>
        /// <returns></returns>

        public void GamepadVibration(float vibTime)
        {
            if (inputDevice == InputDevice.Joystick)
            {
                StartCoroutine(GamepadVibrationRotine(vibTime));
            }
        }
	    
        private IEnumerator GamepadVibrationRotine(float vibTime)
        {
            if (inputDevice == InputDevice.Joystick)
            {
            	#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            	
                XInputDotNetPure.GamePad.SetVibration(0, 1, 1);
                yield return new WaitForSeconds(vibTime);
	            XInputDotNetPure.GamePad.SetVibration(0, 0, 0);
	            
	            #else
	            yield return new WaitForSeconds(0f);
	            #endif
            }
        }

        void OnGUI()
        {
            switch (inputDevice)
            {
                case InputDevice.MouseKeyboard:
                    if (isJoystickInput())
                    {
                        inputDevice = InputDevice.Joystick;

                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
                        }
                    }
                    else if (isMobileInput())
                    {
                        inputDevice = InputDevice.Mobile;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
                        }
                    }
                    break;
                case InputDevice.Joystick:
                    if (isMouseKeyboard())
                    {
                        inputDevice = InputDevice.MouseKeyboard;
                        if (hud != null)
                        {
                            hud.controllerInput = false;
                            hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
                        }
                    }
                    else if (isMobileInput())
                    {
                        inputDevice = InputDevice.Mobile;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
                        }
                    }
                    break;
                case InputDevice.Mobile:
                    if (isMouseKeyboard())
                    {
                        inputDevice = InputDevice.MouseKeyboard;
                        if (hud != null)
                        {
                            hud.controllerInput = false;
                            hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
                        }
                    }
                    else if (isJoystickInput())
                    {
                        inputDevice = InputDevice.Joystick;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
                        }
                    }
                    break;
            }
        }

        private bool isMobileInput()
        {
#if UNITY_EDITOR && UNITY_MOBILE
            if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                return true;
            }
		
#elif MOBILE_INPUT
            if (EventSystem.current.IsPointerOverGameObject() || (Input.touches.Length > 0))
                return true;
#endif
            return false;
        }

        private bool isMouseKeyboard()
        {
#if MOBILE_INPUT
                return false;
#else
            // mouse & keyboard buttons
            if (Event.current.isKey || Event.current.isMouse)
                return true;
            // mouse movement
            if (Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f)
                return true;

            return false;
#endif
        }

        private bool isJoystickInput()
        {
            // joystick buttons
            if (Input.GetKey(KeyCode.Joystick1Button0) ||
                Input.GetKey(KeyCode.Joystick1Button1) ||
                Input.GetKey(KeyCode.Joystick1Button2) ||
                Input.GetKey(KeyCode.Joystick1Button3) ||
                Input.GetKey(KeyCode.Joystick1Button4) ||
                Input.GetKey(KeyCode.Joystick1Button5) ||
                Input.GetKey(KeyCode.Joystick1Button6) ||
                Input.GetKey(KeyCode.Joystick1Button7) ||
                Input.GetKey(KeyCode.Joystick1Button8) ||
                Input.GetKey(KeyCode.Joystick1Button9) ||
                Input.GetKey(KeyCode.Joystick1Button10) ||
                Input.GetKey(KeyCode.Joystick1Button11) ||
                Input.GetKey(KeyCode.Joystick1Button12) ||
                Input.GetKey(KeyCode.Joystick1Button13) ||
                Input.GetKey(KeyCode.Joystick1Button14) ||
                Input.GetKey(KeyCode.Joystick1Button15) ||
                Input.GetKey(KeyCode.Joystick1Button16) ||
                Input.GetKey(KeyCode.Joystick1Button17) ||
                Input.GetKey(KeyCode.Joystick1Button18) ||
                Input.GetKey(KeyCode.Joystick1Button19))
            {
                return true;
            }

            // joystick axis
            if (Input.GetAxis("LeftAnalogHorizontal") != 0.0f ||
                Input.GetAxis("LeftAnalogVertical") != 0.0f ||
                Input.GetAxis("RightAnalogHorizontal") != 0.0f ||
                Input.GetAxis("RightAnalogVertical") != 0.0f ||
                Input.GetAxis("LT") != 0.0f ||
                Input.GetAxis("RT") != 0.0f ||
                Input.GetAxis("D-Pad Horizontal") != 0.0f ||
                Input.GetAxis("D-Pad Vertical") != 0.0f)
            {
                return true;
            }
            return false;
        }

        void OnChangeInput()
        {
            if (onChangeInputType != null)
            {
                onChangeInputType(inputDevice);
            }
        }
    }

    /// <summary>
    /// INPUT TYPE - check in real time if you are using a joystick, mobile or mouse/keyboard
    /// </summary>
    [HideInInspector]
    public enum InputDevice
    {
        MouseKeyboard,
        Joystick,
        Mobile
    };

    [System.Serializable]
    public class GenericInput
    {        
        protected InputDevice inputDevice { get { return vInput.instance.inputDevice; } }
        public bool useInput = true;
        [SerializeField]
        private bool isAxisInUse;
        
        [SerializeField]
        private string keyboard;
        [SerializeField]
        private bool keyboardAxis;
        [SerializeField]
        private string joystick;
        [SerializeField]
        private bool joystickAxis;       
        [SerializeField]
        private string mobile;
        [SerializeField]
        private bool mobileAxis;

        [SerializeField]
        private bool joystickAxisInvert;
        [SerializeField]
        private bool keyboardAxisInvert;
        [SerializeField]
        private bool mobileAxisInvert;

        private float buttomTimer;
        private bool inButtomTimer;
        private float multTapTimer;
        private int multTapCounter;

        public bool isAxis
        {
            get
            {
                bool value = false;
                switch(inputDevice)
                {
                    case InputDevice.Joystick:
                        value = joystickAxis;
                        break;
                    case InputDevice.MouseKeyboard:
                        value = keyboardAxis;
                        break;
                    case InputDevice.Mobile:
                        value = mobileAxis;
                        break;
                }
                return value;
            }
        }

        public bool isAxisInvert
        {
            get
            {
                bool value = false;
                switch (inputDevice)
                {
                    case InputDevice.Joystick:
                        value = joystickAxisInvert;
                        break;
                    case InputDevice.MouseKeyboard:
                        value = keyboardAxisInvert;
                        break;
                    case InputDevice.Mobile:
                        value = mobileAxisInvert;
                        break;
                }
                return value;
            }
        }

        /// <summary>
        /// Initialise a new GenericInput
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="joystick"></param>
        /// <param name="mobile"></param>
        public GenericInput(string keyboard, string joystick, string mobile)
        {
            this.keyboard = keyboard;
            this.joystick = joystick;
            this.mobile = mobile;
        }

        /// <summary>
        /// Initialise a new GenericInput
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="joystick"></param>
        /// <param name="mobile"></param>
        public GenericInput(string keyboard, bool keyboardAxis, string joystick, bool joystickAxis, string mobile, bool mobileAxis)
        {
            this.keyboard = keyboard;
            this.keyboardAxis = keyboardAxis;
            this.joystick = joystick;
            this.joystickAxis = joystickAxis;
            this.mobile = mobile;
            this.mobileAxis = mobileAxis;
        }

        /// <summary>
        /// Button Name
        /// </summary>
        public string buttonName
        {
            get
            {
                if (vInput.instance != null)
                {
                    if (vInput.instance.inputDevice == InputDevice.MouseKeyboard) return keyboard.ToString();
                    else if (vInput.instance.inputDevice == InputDevice.Joystick) return joystick;
                    else return mobile;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if button is a Key
        /// </summary>
        public bool isKey
        {
            get
            {
                if (vInput.instance != null)
                {
                    if (System.Enum.IsDefined(typeof(KeyCode), buttonName))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Get <see cref="KeyCode"/> value
        /// </summary>
        public KeyCode key
        {
            get
            {
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), buttonName);
            }
        }

        /// <summary>
        /// Get Button
        /// </summary>
        /// <returns></returns>
        public bool GetButton()
        {            
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (isAxis) return GetAxisButton();

            // mobile
            if (inputDevice == InputDevice.Mobile)
            {
                #if MOBILE_INPUT
                if (CrossPlatformInputManager.GetButton(this.buttonName))
                #endif
                    return true;
            }
            // keyboard/mouse
            else if (inputDevice == InputDevice.MouseKeyboard)
            {
                if (isKey)
                {
                    if (Input.GetKey(key))
                        return true;
                }
                else
                {
                    if (Input.GetButton(this.buttonName))
                        return true;
                }
            }
            // joystick
            else if (inputDevice == InputDevice.Joystick)
            {
                if (Input.GetButton(this.buttonName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get ButtonDown
        /// </summary>
        /// <returns></returns>
        public bool GetButtonDown()
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (isAxis) return GetAxisButtonDown();
            // mobile
            if (inputDevice == InputDevice.Mobile)
            {
                #if MOBILE_INPUT
                if (CrossPlatformInputManager.GetButtonDown(this.buttonName))
                #endif
                    return true;
            }
            // keyboard/mouse
            else if (inputDevice == InputDevice.MouseKeyboard)
            {
                if (isKey)
                {
                    if (Input.GetKeyDown(key))
                        return true;
                }
                else
                {
                    if (Input.GetButtonDown(this.buttonName))
                        return true;
                }
            }
            // joystick
            else if (inputDevice == InputDevice.Joystick)
            {
                if (Input.GetButtonDown(this.buttonName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Button Up
        /// </summary>
        /// <returns></returns>
        public bool GetButtonUp()
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (isAxis) return GetAxisButtonUp();

            // mobile
            if (inputDevice == InputDevice.Mobile)
            {
                #if MOBILE_INPUT
                if (CrossPlatformInputManager.GetButtonUp(this.buttonName))
                #endif
                    return true;
            }
            // keyboard/mouse
            else if (inputDevice == InputDevice.MouseKeyboard)
            {
                if (isKey)
                {
                    if (Input.GetKeyUp(key))
                        return true;
                }
                else
                {
                    if (Input.GetButtonUp(this.buttonName))
                        return true;
                }
            }
            // joystick
            else if (inputDevice == InputDevice.Joystick)
            {
                if (Input.GetButtonUp(this.buttonName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Axis
        /// </summary>
        /// <returns></returns>
        public float GetAxis()
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName) || isKey) return 0;

            // mobile
            if (inputDevice == InputDevice.Mobile)
            {
                #if MOBILE_INPUT
                return CrossPlatformInputManager.GetAxis(this.buttonName);
                #endif
            }
            // keyboard/mouse
            else if (inputDevice == InputDevice.MouseKeyboard)
            {
                return Input.GetAxis(this.buttonName);
            }
            // joystick
            else if (inputDevice == InputDevice.Joystick)
            {
                return Input.GetAxis(this.buttonName);
            }
            return 0;
        }

        /// <summary>
        /// Get Axis Raw
        /// </summary>
        /// <returns></returns>
        public float GetAxisRaw()
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName) || isKey) return 0;

            // mobile
            if (inputDevice == InputDevice.Mobile)
            {
                #if MOBILE_INPUT
                return CrossPlatformInputManager.GetAxisRaw(this.buttonName);
                #endif
            }
            // keyboard/mouse
            else if (inputDevice == InputDevice.MouseKeyboard)
            {
                return Input.GetAxisRaw(this.buttonName);
            }
            // joystick
            else if (inputDevice == InputDevice.Joystick)
            {
                return Input.GetAxisRaw(this.buttonName);
            }
            return 0;
        }

        /// <summary>
        /// Get Double Button Down Check if button is pressed Within the defined time
        /// </summary>
        /// <param name="inputTime"></param>
        /// <returns></returns>
        public bool GetDoubleButtonDown(float inputTime = 1)
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;

            if (multTapCounter == 0 && GetButtonDown())
            {
                multTapTimer = Time.time;
                multTapCounter = 1;
                return false;
            }

            if (multTapCounter == 1 && GetButtonDown())
            {
                var time = multTapTimer + inputTime;
                var valid = (Time.time < time);
                multTapTimer = 0;
                multTapCounter = 0;
                return valid;
            }
            return false;
        }

        /// <summary>
        /// Get Buttom Timer Check if button is pressed for defined time
        /// </summary>
        /// <param name="inputTime"> time to check button press</param>
        /// <returns></returns>
        public bool GetButtonTimer(float inputTime = 2)
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (GetButtonDown() && !inButtomTimer)
            {
                buttomTimer = Time.time;
                inButtomTimer = true;
            }
            if (inButtomTimer)
            {
                var time = buttomTimer + inputTime;
                var valid = (time - Time.time <= 0);
                if (GetButtonUp())
                {
                    inButtomTimer = false;
                    return valid;
                }
                if (valid)
                {
                    inButtomTimer = false;
                }
                return valid;
            }
            return false;
        }

        /// <summary>
        /// Get Axis like a button        
        /// </summary>
        /// <param name="value">Value to check need to be diferent 0</param>
        /// <returns></returns>
        public bool GetAxisButton(float value = 0.5f)
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (isAxisInvert) value *= -1f;
            if (value > 0)
            {
                return GetAxisRaw() >= value;
            }
            else if (value < 0)
            {
                return GetAxisRaw() <= value;
            }
            return false;
        }

        /// <summary>
        /// Get Axis like a buttonDown        
        /// </summary>
        /// <param name="value">Value to check need to be diferent 0</param>
        /// <returns></returns>
        public bool GetAxisButtonDown(float value = 0.5f)
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(this.buttonName)) return false;
            if (isAxisInvert) value *= -1f;
            if (value > 0)
            {
                if (!isAxisInUse && GetAxisRaw() >= value)
                {
                    isAxisInUse = true;
                    return true;
                }
                else if (isAxisInUse && GetAxisRaw() == 0)
                {
                    isAxisInUse = false;
                }
            }
            else if (value < 0)
            {
                if (!isAxisInUse && GetAxisRaw() <= value)
                {
                    isAxisInUse = true;
                    return true;
                }
                else if (isAxisInUse && GetAxisRaw() == 0)
                {
                    isAxisInUse = false;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Axis like a buttonUp
        /// Check if Axis is zero after press       
        /// <returns></returns>
        public bool GetAxisButtonUp()
        {            
            if (isAxisInUse && GetAxisRaw() == 0)
            {
                isAxisInUse = false;
                return true;
            }
            else if (!isAxisInUse && GetAxisRaw() != 0)
            {
                isAxisInUse = true;
            }
            return false;
        }

        bool IsButtonAvailable(string btnName)
        {
            if (!useInput) return false;
            try
            {
                if (isKey) return true;
                Input.GetButton(buttonName);
                return true;
            }
            catch (System.Exception exc)
            {
                Debug.LogWarning(" Failure to try access button :" + buttonName + "\n" + exc.Message);
                return false;
            }
        }

    }
}