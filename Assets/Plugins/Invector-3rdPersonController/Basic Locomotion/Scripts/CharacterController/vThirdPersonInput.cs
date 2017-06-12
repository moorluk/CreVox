using UnityEngine;
using System.Collections;
#if MOBILE_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace Invector.CharacterController
{
    [vClassHeader("Input Manager")]
    public class vThirdPersonInput : vMonoBehaviour
    {       
        #region Variables

        public GameplayInputStyle gameplayInputStyle = GameplayInputStyle.DirectionalInput;
        public LayerMask clickMoveLayer = 1 << 0;
        public bool inverseInputDirection;
        public bool lockInput;
        public bool lockCamera;

        [Header("Default Inputs")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        public GenericInput jumpInput = new GenericInput("Space", "X", "X");
        public GenericInput rollInput = new GenericInput("Q", "B", "B");
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput crouchInput = new GenericInput("C", "Y", "Y");       

        [Header("Camera Settings")]
        public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");
        public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");
        public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");

        protected vThirdPersonCamera tpCamera;              // acess camera info                
        [HideInInspector]
        public string customCameraState;                    // generic string to change the CameraState        
        [HideInInspector]
        public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode        
        [HideInInspector]
        public bool changeCameraState;                      // generic bool to change the CameraState        
        [HideInInspector]
        public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp  
        [HideInInspector]
        public bool keepDirection;                          // keep the current direction in case you change the cameraState
        protected Vector2 oldInput;

        // isometric cursor
        public delegate void OnEnableCursor(Vector3 position);
        public delegate void OnDisableCursor();
        public OnEnableCursor onEnableCursor;
        public OnDisableCursor onDisableCursor;
        [HideInInspector]
        public Vector3 cursorPoint;
        // isometric cursor

        [HideInInspector]
        public vThirdPersonController cc;                   // access the ThirdPersonController component
        [HideInInspector]
        public vHUDController hud;                          // acess vHUDController component        
        


        public enum GameplayInputStyle
        {
            ClickAndMove,
            DirectionalInput
        }

        protected InputDevice inputDevice { get { return vInput.instance.inputDevice; } }

        #endregion        
	    
        #region Initialize Character, Camera & HUD when LoadScene
	    
        protected virtual void Start()
	    {
		    cc = GetComponent<vThirdPersonController>();
		    if(vThirdPersonController.instance == cc || vThirdPersonController.instance == null)
		    {
                #if UNITY_5_4_OR_NEWER
                SceneManager.sceneLoaded += OnLevelFinishedLoading;
                #endif
			    
			    CharacterInit();
		    }
	    }
	    
        #if UNITY_5_4_OR_NEWER
        	    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	    {
    		try
    		{
	    		CharacterInit();
			}
	    	catch
	    	{
		    	SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	    	}
        }
        #else
	    public void OnLevelWasLoaded(int level)
	    {
    		try
    		{
	    		cc = GetComponent<vThirdPersonController>();
	    		if (vThirdPersonController.instance == cc || vThirdPersonController.instance == null)
		    		CharacterInit();
    		}
	    		catch
	    		{
		    		
	    		}
	    }
        #endif
	    
	    protected virtual void CharacterInit()
	    {
	    	if(cc != null)
		    	cc.Init();
		    
		    tpCamera = FindObjectOfType<vThirdPersonCamera>();
		    if (tpCamera) tpCamera.SetMainTarget(this.transform);
		    
		    cursorPoint = transform.position;		   
		    //Cursor.visible = false;
		    //Cursor.lockState = CursorLockMode.Locked;
		    
		    hud = vHUDController.instance;
		    if (hud != null)
			    hud.Init(cc);
	    }
        #endregion

	    protected virtual void LateUpdate()
	    {
		    if (cc == null || lockInput || Time.timeScale == 0) return;
		    InputHandle();                      // update input methods          
            UpdateCameraStates();               // update camera states            
        }
	    
	    protected virtual void FixedUpdate()
	    {
		    cc.AirControl();
            CameraInput();
            MoveToPoint();
        }

        protected virtual void Update()
	    {
		    cc.UpdateMotor();               	// call ThirdPersonMotor methods               
		    cc.UpdateAnimator();            	// call ThirdPersonAnimator methods
		    UpdateHUD();                        // update hud graphics		             
        }       

        protected virtual void InputHandle()
        {
            ExitGameInput();

            if (!cc.lockMovement && !cc.ragdolled)
            {
                MoveCharacter();
                SprintInput();
                CrouchInput();
                StrafeInput();
                JumpInput();
                RollInput();
            }
        }

        #region Generic Methods
        // you can use these methods with Playmaker or AdventureCreator to have better control on cutscenes and events.

        /// <summary>
        /// Lock all the Input from the Player
        /// </summary>
        /// <param name="value"></param>
        public void LockInput(bool value)
        {
            lockInput = value;
            if (value) cc.input = Vector2.zero;
        }

        /// <summary>
        /// Show/Hide Cursor
        /// </summary>
        /// <param name="value"></param>
        public void ShowCursor(bool value)
        {
            Cursor.visible = value;
        }

        /// <summary>
        /// Lock the Camera Input
        /// </summary>
        /// <param name="value"></param>
        public void LockCamera(bool value)
        {
            lockCamera = value;
        }

        /// <summary>
        /// Limits the character to walk only, useful for cutscenes and 'indoor' areas
        /// </summary>
        /// <param name="value"></param>
        public void LimitToWalk(bool value)
        {
            cc.freeWalkByDefault = value;
            cc.strafeWalkByDefault = value;
        }
        
        /// <summary>
        /// If you need you character to move automatically on a cutscene, always change the gamePlayInputStyle to Click and Move
        /// </summary>
        public void SetClickAndMove()
        {
            gameplayInputStyle = GameplayInputStyle.ClickAndMove;
            cursorPoint = transform.position;
            cc.rotateByWorld = true;
        }

        /// <summary>
        /// Always set the Player back to DirectionalInput if you're using normal 3rd Person View with the rotation based at the camera.
        /// If you're using Topdown or Isometric view, make sure to check the option rotateByWorld.
        /// </summary>
        public void SetDirectionalInput()
        {
            gameplayInputStyle = GameplayInputStyle.DirectionalInput;
            cc.rotateByWorld = false;
        }

        #endregion

        #region Basic Locomotion Inputs      

        protected virtual void MoveCharacter()
        {
            if (gameplayInputStyle == GameplayInputStyle.ClickAndMove)
            {
                cc.rotateByWorld = true;
                ClickAndMove();
            }
            else
                ControllerInput();
        }

        protected virtual void ControllerInput()
        {
            // gets input from mobile           
            cc.input.x = horizontalInput.GetAxis();
            cc.input.y = verticallInput.GetAxis();
            // update oldInput to compare with current Input if keepDirection is true
            if (!keepDirection)
                oldInput = cc.input;
        }

        protected virtual void StrafeInput()
        {
            if (strafeInput.GetButtonDown())
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (sprintInput.GetButtonDown())
                cc.Sprint(true);
            else
                cc.Sprint(false);
        }

        protected virtual void CrouchInput()
        {
            if (crouchInput.GetButtonDown())
                cc.Crouch();
        }

        protected virtual void JumpInput()
        {
            if (jumpInput.GetButtonDown())
                cc.Jump();
        }

        protected virtual void RollInput()
        {
            if (rollInput.GetButtonDown())
                cc.Roll();
        } 

        protected virtual void ExitGameInput()
        {
            // just a example to quit the application 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Cursor.visible)
                    Cursor.visible = true;
                else
                    Application.Quit();
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            cc.CheckTriggers(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            cc.CheckTriggerExit(other);
        }

        #endregion

        #region TopDown Methods

        protected virtual void ClickAndMove()
        {           
            RaycastHit hit;

            if (Input.GetMouseButton(0))
            {
                if (Physics.Raycast(tpCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, clickMoveLayer))
                {
                    if (onEnableCursor != null)
                    {
                        onEnableCursor(hit.point);
                    }
                    cursorPoint = hit.point;
                }
            }           
        }

        protected void MoveToPoint()
        {
            if (gameplayInputStyle != GameplayInputStyle.ClickAndMove) return;

            var dir = (cursorPoint - transform.position).normalized;

            if (!NearPoint(cursorPoint, transform.position))
                cc.input = new Vector2(dir.x, dir.z);
            else
            {
                if (onDisableCursor != null)
                    onDisableCursor();

                cc.input = Vector2.Lerp(cc.input, Vector3.zero, 20 * Time.deltaTime);
            }
        }

        public void SetTargetPosition(Vector3 value)
        {
            cursorPoint = value;
            var dir = (value - transform.position).normalized;
            cc.input = new Vector2(dir.x, dir.z);
        }

        public void ClearTarget()
        {
            cc.input = Vector2.zero;
        }

        protected virtual bool NearPoint(Vector3 a, Vector3 b)
        {
            var _a = new Vector3(a.x, transform.position.y, a.z);
            var _b = new Vector3(b.x, transform.position.y, b.z);
            return Vector3.Distance(_a, _b) <= 0.5f;
        }

        #endregion

        #region Camera Methods

        public virtual void CameraInput()
        {
            if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
            if (!keepDirection) cc.UpdateTargetDirection(Camera.main.transform);
            RotateWithCamera(Camera.main.transform);

            if (tpCamera == null || lockCamera)
                return;
            var Y = rotateCameraYInput.GetAxis();
            var X = rotateCameraXInput.GetAxis();
            var zoom = cameraZoomInput.GetAxis();

            tpCamera.RotateCamera(X, Y);
            tpCamera.Zoom(zoom);
            
            // change keedDirection from input diference
            if (keepDirection && Vector2.Distance(cc.input, oldInput) > 0.2f) keepDirection = false;
        }
       
        protected virtual void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData

            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }

            if (changeCameraState && !cc.isStrafing)
                tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
            else if (cc.isCrouching)
                tpCamera.ChangeState("Crouch", true);
            else if (cc.isStrafing)
                tpCamera.ChangeState("Strafing", true);
            else
                tpCamera.ChangeState("Default", true);
        }

        protected virtual void RotateWithCamera(Transform cameraTransform)
        {            
            if (cc.isStrafing && !cc.actions && !cc.lockMovement)
            {                
                // smooth align character with aim position               
                if (tpCamera != null && tpCamera.lockTarget)
                {
                    cc.RotateToTarget(tpCamera.lockTarget);
                }
                // rotate the camera around the character and align with when the char move
                else if(cc.input != Vector2.zero)
                {
                    cc.RotateWithAnotherTransform(cameraTransform);
                }
            }
        }

        #endregion

        #region HUD       

        public virtual void UpdateHUD()
        {
            if (hud == null)
                return;

            hud.UpdateHUD(cc);
        }

        #endregion
    }
}