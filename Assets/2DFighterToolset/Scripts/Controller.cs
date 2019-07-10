using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*!
    An abstraction of the joystick input. numeric values from the joystick are converted into values from this enum in the controller class.
 */
public enum Directions {
	LEFT,
	LEFTDOWN,
	DOWN,
	RIGHTDOWN,
	RIGHT,
	RIGHTUP,
	UP,
	LEFTUP,
	CENTRED,
    NULL
};

/*!
    An abstraction of controller button input. values from button axes are converted into values from this enum in the controller class. All button mapping can be changed in the input manager in the project settings
 */
public enum Buttons {
	LIGHTPUNCH, //!< defaultly assigned to the A button on an xbox controller
	MEDIUMPUNCH, //!< defaultly assigned to the X button on an xbox controller
    HEAVYPUNCH, //!< defaultly assigned to the LB button on an xbox controller
    ANYPUNCH,
    LIGHTKICK, //!< defaultly assigned to the B button on an xbox controller
    MEDIUMKICK, //!< defaultly assigned to the Y button on an xbox controller
    HEAVYKICK, //!< defaultly assigned to the RB button on an xbox controller
    ANYKICK,
    NULL
}
	
/*!
    A container for an input from a controller. Stored in a list in the controller class. When the constructor is called the time is recorded for determining if a motion is valid or not.
 */
public struct DirectionalInput
{
	public Directions direction;//! the direction from the controller
	public Buttons button;//! the button press from the controller
	public float timeOfInput;

    /*!
        Constructor for a directional input. automatically records it's time of input on construction
        \param _direction, the direction that will be recorded
        \param _button, the button that will be recorded
    */
	public DirectionalInput(Directions _direction, Buttons _button)
	{
        direction = _direction;
		button = _button;
        timeOfInput = Time.realtimeSinceStartup;
	}	
}

/*!
    An abstraction between the user and the input manager. Use this for controller input
 */
public class Controller : MonoBehaviour {

    public PlayerNumber PlayerControllerNumber;//!<defines which controller is currently being monitored by this instance of the controller class.

    public List<DirectionalInput> RecentInputs;//!<A list of the 30 most recent inputs from the controller, used for animation triggering and detecting if a special move has been executed (See Fighter class)
	public delegate void ControllerAction();
	public event ControllerAction JoystickMoved;//!<Event that is called whenever RecentInputs list is updated.

    /*!
        List of controllers that can be monitored, currently only supports 2 controllers. 
    */
	public enum PlayerNumber{
		P1,
		P2
	};

	// Use this for initialization
	void Start () {
        RecentInputs = new List<DirectionalInput>();
	}
	
	/*!
         Checks the controller to see if any new inputs have occured. If they have they are added to the recent inputs list, if there is a new input, the joystickmoved event is called.
    */
	void Update ()
    {
		DetermineButton ();
        if(PlayerControllerNumber == PlayerNumber.P1)
		{
			DirectionalInput di = new DirectionalInput(DetermineDirection("HorizontalP1", "VerticalP1"), DetermineButton());
			if (IsNewDirection(di))
            {
                RecentInputs.Insert(0, di);
                if(RecentInputs.Count > 30)
                {
                    RecentInputs.RemoveAt(29);
                }
				JoystickMoved ();
            }
        }
        else if(PlayerControllerNumber == PlayerNumber.P2)
        {
			DirectionalInput di = new DirectionalInput(DetermineDirection("HorizontalP2", "VerticalP2"), DetermineButton());
			if (IsNewDirection(di))
            {
				RecentInputs.Insert(0, di);
                if (RecentInputs.Count > 30)
                {
                    RecentInputs.RemoveAt(29);
                }
				JoystickMoved ();
            }
        }

	}

    /*!
        Determines if the inputted direction is a new one, and therefore if it should be added to the recent inputs list. Checks the new input against the last to see if anything has changed. Called in the update function
        \param _DirectionToCheck the new inpur from the controller.
    */
	bool IsNewDirection(DirectionalInput _DirectionToCheck)
	{

		if (PlayerControllerNumber == PlayerNumber.P1)
		{
			if (_DirectionToCheck.direction == Directions.NULL)
			{
				return false;
			}

			if (RecentInputs.Count < 1)
			{
				return true;
			}

			if (_DirectionToCheck.button != RecentInputs [0].button)
			{
				return true;
			}

			if (_DirectionToCheck.direction != RecentInputs [0].direction)
			{
				return true;
			}

			return false;
		}

		else
		{
			if (_DirectionToCheck.direction == Directions.NULL)
			{
				return false;
			}

			if (RecentInputs.Count < 1)
			{
				return true;
			}

			if (_DirectionToCheck.button != RecentInputs [0].button)
			{
				return true;
			}

			if (_DirectionToCheck.direction != RecentInputs [0].direction)
			{
				return true;
			}

			return false;
		}
	}

    /*!
        Returns a value from the direction enumerator based on what values are returned from the input axes.
    */
   Directions DetermineDirection(string horizontalAxisString, string verticalAxisString)
    {
        //diagonal inputs
        if(Input.GetAxis(horizontalAxisString) >= 1.0f && Input.GetAxis(verticalAxisString) <= -1.0f)
        {
            return Directions.RIGHTUP;
        }
        else if(Input.GetAxis(horizontalAxisString) >= 1.0f && Input.GetAxis(verticalAxisString) >= 1.0f)
        {
            return Directions.RIGHTDOWN;
        }
        else if(Input.GetAxis(horizontalAxisString) <= -1.0f && Input.GetAxis(verticalAxisString) <= -1.0f)
        {
            return Directions.LEFTUP;
        }
        else if (Input.GetAxis(horizontalAxisString) <= -1.0f && Input.GetAxis(verticalAxisString) >= 1.0f)
        {
            return Directions.LEFTDOWN;
        }

        //horizontal & vertical inputs
        else if (Input.GetAxis(horizontalAxisString) <= -1.0f)
        {
            return Directions.LEFT;
        }
        else if (Input.GetAxis(horizontalAxisString) >= 1.0f)
        {
            return Directions.RIGHT;
        }
        else if (Input.GetAxis(verticalAxisString) >= 1.0f)
        {
            return Directions.DOWN;
        }
        else if (Input.GetAxis(verticalAxisString) <= -1.0f)
        {
            return Directions.UP;
        }

        else
        {
			return Directions.CENTRED;
        }
    }

    /*!
        Returns a value from the button enumerator based on what values are returned from the button input axes 
    */
	Buttons DetermineButton()
	{
		if (PlayerControllerNumber == PlayerNumber.P1)
		{
			if (Input.GetAxis ("Light PunchP1") > 0)
			{
				return Buttons.LIGHTPUNCH;
			}
			else if (Input.GetAxis ("Medium PunchP1") > 0)
			{
				return Buttons.MEDIUMPUNCH;
			}
			else if (Input.GetAxis ("Heavy PunchP1") > 0)
			{
				return Buttons.HEAVYPUNCH;
			}
			else if (Input.GetAxis ("Light KickP1") > 0)
			{
				return Buttons.LIGHTKICK;
			}
			else if (Input.GetAxis ("Medium KickP1") > 0)
			{
				return Buttons.MEDIUMKICK;
			}
			else if (Input.GetAxis ("Heavy KickP1") > 0)
			{
				return Buttons.HEAVYKICK;
			}
			else
			{
				return Buttons.NULL;
			}
		}
		else
		{
			if (Input.GetAxis ("Light PunchP2") > 0)
			{
				return Buttons.LIGHTPUNCH;
			}
			else if (Input.GetAxis ("Medium PunchP2") > 0)
			{
				return Buttons.MEDIUMPUNCH;
			}
			else if (Input.GetAxis ("Heavy PunchP2") > 0)
			{
				return Buttons.HEAVYPUNCH;
			}
			else if (Input.GetAxis ("Light KickP2") > 0)
			{
				return Buttons.LIGHTKICK;
			}
			else if (Input.GetAxis ("Medium KickP2") > 0)
			{
				return Buttons.MEDIUMKICK;
			}
			else if (Input.GetAxis ("Heavy KickP2") > 0)
			{
				return Buttons.HEAVYKICK;
			}
			else
			{
				return Buttons.NULL;
			}
		}
	}

}
