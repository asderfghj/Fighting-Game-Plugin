using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*! Fighter direction
    Represents the direction in which a fighter faces, used for determining if a motion should be reversed.
 */
public enum FighterDirection
{
	Right,
	Left
}

/*! Animation Parameter Enum
    Used to define animation parameter datatypes for triggering animations.
 */
public enum AnimationParameterDatatype
{
	Trigger,
	Bool,
	Int,
	Float
}

/*! 
    This class is used to process hitbox data and analyse controller input to execute special moves on the fighter. This class also acts as an intermediary for Unity's animation tools. 
 */
public class Fighter : MonoBehaviour {

	public List<SpecialMove> Moveset;
	public TextAsset HitboxJSONData; //!< Used to parse JSON data from the hitbox editor and add box collider to the fighter. Press the add hitboxes button when this variable has a value that is not null.
	public Controller MonitoredController;
	public FighterDirection CurrentDirection; //!< used to determine if a move should be reversed. 
	public AnimationParameter WalkingParameter, CrouchingParameter, LPunchParameter, MPunchParameter, HPunchParameter, LKickParameter, MKickParameter, HKickParameter;
	public Animator AnimationController;

	void Start()
	{
		MonitoredController.JoystickMoved += CheckForMove;
	}

	/*!
		Activated in editor with the add hitboxes button, used for adding hitboxes. Will not work if the hitboxJSONData varible is null
	*/
	public void AddHitboxes()
	{
		bool addHitboxes = true, addHurtboxes = true;
		GameObject HitboxGO = new GameObject(), HurtboxGO = new GameObject(), CollisionContainer = new GameObject();
        boundingBoxes boundingInfo = CreateFromJSON();

        CollisionContainer.name = boundingInfo.Name;

        for (int i = 0; i < transform.childCount; i++) 
		{
			if (transform.GetChild (i).name == boundingInfo.Name) 
			{
				DestroyImmediate (transform.GetChild(i));
			}
		}

        if(boundingInfo.Hitboxes.Count < 1)
        {
            addHitboxes = false;
            DestroyImmediate(HitboxGO);
        }

        if(boundingInfo.Hurtboxes.Count < 1)
        {
            addHurtboxes = false;
            DestroyImmediate(HurtboxGO);
        }
		
		if (addHitboxes) 
		{
			HitboxGO.name = "HitBoxes";
			HitboxGO.transform.SetParent (CollisionContainer.transform);
            foreach (HitboxData hbd in boundingInfo.Hitboxes)
            {
                BoxCollider2D hb = HitboxGO.AddComponent<BoxCollider2D>();
                hb.offset = new Vector2(hbd.x, hbd.y);
                hb.size = new Vector2(hbd.w, hbd.h);
            }
            HitboxGO.transform.position = new Vector2(0, 0);
        }


		if (addHurtboxes)
		{
			HurtboxGO.name = "HurtBoxes";
			HurtboxGO.transform.SetParent (CollisionContainer.transform);
            foreach (HitboxData hbd in boundingInfo.Hurtboxes)
            {
                BoxCollider2D hb = HurtboxGO.AddComponent<BoxCollider2D>();
                hb.offset = new Vector2(hbd.x, hbd.y);
                hb.size = new Vector2(hbd.w, hbd.h);
            }
            HurtboxGO.transform.position = new Vector2(0, 0);
        }
			
        CollisionContainer.transform.position = new Vector2(0, 0);
        CollisionContainer.transform.SetParent(transform);
		CollisionContainer.SetActive (false);
	}

	/*!
		Creates the hitbox data from a json file created in the hitbox editor.
	*/
	private boundingBoxes CreateFromJSON()
	{
		return JsonUtility.FromJson<boundingBoxes>(HitboxJSONData.text);
	}

	/*!
		Goes through the most recent inputs from the monitored controller and checks if a special move is triggered. If it is not, this function calls the PerformStandardAction function.
	*/
	void CheckForMove()
	{
		List<SpecialMove> ActivatedMoves = new List<SpecialMove> ();
		for (int i = 0; i < Moveset.Count; i++)//outer count, going through each move in the moveset
		{
			if (Moveset [i].Motion.ManoeuvreDirections.Count + 1 < MonitoredController.RecentInputs.Count)//checks that the move is actually possible with the current number of inputs.
			{
				bool MoveInputted = false, first = true;
				float startTime = 0.0f, endTime = 0.0f;
				List<DirectionalInput> Inputs = GetRelevantInputs (Moveset [i].Motion.ManoeuvreDirections.Count + 1, Moveset [i].Motion.ignoreCenterPositions);
				for (int j = 0; j < Inputs.Count; j++)
				{
					if (Inputs.Count < Moveset [i].Motion.ManoeuvreDirections.Count + 1)
					{
						break;
					}

					if (first) {
						startTime = Inputs [j].timeOfInput;
						first = false;
					}

					if (j == Inputs.Count - 1)//checking for button press
					{
						if (Moveset [i].Button == Buttons.ANYKICK || Moveset [i].Button == Buttons.ANYPUNCH)
						{
							if (AnyKickPunchCheck (Moveset [i].Button, Inputs[j].button))
							{
								MoveInputted = true;
								endTime = Inputs [j].timeOfInput;
							}
						}
						else if (Moveset [i].Button == Inputs[j].button) //if the button matches then that means that everything has matched so the move was inputted
						{
							MoveInputted = true;
							endTime = Inputs [j].timeOfInput;
						}
					}
					else if (ConvertDirection(Moveset [i].Motion.ManoeuvreDirections[j], Moveset [i].Motion.defaultDirection) != Inputs [j].direction)//checking for correct direction
					{
						//Debug.Log(string.Format("Directions do not match: input: {0}, requested: {1}", Inputs [j].direction, motions [i].ManoeuvreDirections [j]));
						break;

					}

				}

				if (MoveInputted) //add to a separate list as multiple inputs may have been completed
				{
					if (endTime - startTime <= Moveset [i].InputSpeed)
					{
						ActivatedMoves.Add (Moveset [i]);
					}
				}
			}
		}

		if (ActivatedMoves.Count > 0) 
		{
			int InputtedMove = 0;
			for (int i = 0; i < ActivatedMoves.Count; i++) 
			{
				if (ActivatedMoves [i].Motion.ManoeuvreDirections.Count > ActivatedMoves [i].Motion.ManoeuvreDirections.Count) 
				{
					InputtedMove = i;
				}
			}

			Debug.Log (ActivatedMoves [InputtedMove].Name);
            ResetAttackTriggers();
            ActivateAnimationParameter (ActivatedMoves [InputtedMove].AnimParameter);
		} 
		else 
		{
			PerformStandardAction(GetRelevantInputs (1, false)[0]);
		}


	}


	/*!
		Activates if no special move has been activated. triggers an animation if a standard action is inputted (walking, crouching, standard attacks)
	*/
	void PerformStandardAction(DirectionalInput _mostRecentInput)
	{
        if (_mostRecentInput.direction == Directions.LEFT || _mostRecentInput.direction == Directions.RIGHT)
        {
            ActivateAnimationParameter(WalkingParameter);
            DeactivateAnimationParameter(CrouchingParameter);
        }
        else if (_mostRecentInput.direction == Directions.LEFTDOWN || _mostRecentInput.direction == Directions.DOWN || _mostRecentInput.direction == Directions.RIGHTDOWN)
        {
            ActivateAnimationParameter(CrouchingParameter);
            DeactivateAnimationParameter(WalkingParameter);
        }
        else
        {
            DeactivateAnimationParameter(WalkingParameter);
            DeactivateAnimationParameter(CrouchingParameter);
        }

		if (_mostRecentInput.button != Buttons.NULL) 
		{
			switch (_mostRecentInput.button) 
			{
				case Buttons.LIGHTPUNCH:
				{
					ActivateAnimationParameter (LPunchParameter);	
					break;
				}
				case Buttons.MEDIUMPUNCH:
				{
					ActivateAnimationParameter (MPunchParameter);
					break;
				}
				case Buttons.HEAVYPUNCH:
				{
					ActivateAnimationParameter (HPunchParameter);
					break;
				}
				case Buttons.LIGHTKICK:
				{
					ActivateAnimationParameter (LKickParameter);
					break;
				}
				case Buttons.MEDIUMKICK:
				{
					ActivateAnimationParameter (MKickParameter);
					break;
				}
				case Buttons.HEAVYKICK:
				{
					ActivateAnimationParameter (HKickParameter);
					break;
				}
			}
		}

	
	}

	void ActivateAnimationParameter(AnimationParameter animParam)
	{
		if (animParam.paramDatatype == AnimationParameterDatatype.Trigger) 
		{
			AnimationController.SetTrigger (animParam.name);
		}
		else if(animParam.paramDatatype == AnimationParameterDatatype.Bool)
		{
			AnimationController.SetBool (animParam.name, true);
		}
	}

	void DeactivateAnimationParameter(AnimationParameter animParam)
	{
		if(animParam.paramDatatype == AnimationParameterDatatype.Bool)
		{
			AnimationController.SetBool (animParam.name, false);
		}
	}

    void ResetAttackTriggers()
    {
        AnimationController.ResetTrigger(LPunchParameter.name);
        AnimationController.ResetTrigger(MPunchParameter.name);
        AnimationController.ResetTrigger(HPunchParameter.name);
        AnimationController.ResetTrigger(LKickParameter.name);
        AnimationController.ResetTrigger(MKickParameter.name);
        AnimationController.ResetTrigger(HKickParameter.name);
    }

    /*!
        checks if a controller input matches an ANY check  
    */
    bool AnyKickPunchCheck(Buttons _requiredButton, Buttons _inputtedButton)
	{
		if (_requiredButton == Buttons.ANYKICK)
		{
			if (_inputtedButton == Buttons.LIGHTKICK || _inputtedButton == Buttons.MEDIUMKICK || _inputtedButton == Buttons.HEAVYKICK)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else if (_requiredButton == Buttons.ANYPUNCH)
		{
			if (_inputtedButton == Buttons.LIGHTPUNCH || _inputtedButton == Buttons.MEDIUMPUNCH || _inputtedButton == Buttons.HEAVYPUNCH)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			if (_requiredButton == _inputtedButton)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

    /*!
         returns a list of inputs to check for a motion depending on if the motion ignore centre positions
         \param _amount the size of the list that will be returned
         \param ignoreCentre determines whether the returned list will include centre postions, list will still have same size regardless of if this is true or false.
    */
	List<DirectionalInput> GetRelevantInputs(int _amount, bool ignoreCentre)
	{
		List<DirectionalInput> rtn = new List<DirectionalInput>();
		int cap = _amount;

		if (_amount > MonitoredController.RecentInputs.Count)
		{
			return rtn;//can get the amount requested because there have not been enough inputs yet, return an empty list
		}

		for (int i = 0; (i < cap && rtn.Count < _amount) && i < MonitoredController.RecentInputs.Count; i++) 
		{
			if (!ignoreCentre)
			{
				rtn.Insert (0, MonitoredController.RecentInputs [i]);
			}
			else if (MonitoredController.RecentInputs [i].direction != Directions.CENTRED || MonitoredController.RecentInputs [i].button != Buttons.NULL)
			{
				rtn.Insert (0, MonitoredController.RecentInputs [i]);
			}
			else
			{
				cap++;
			}
		}

		return rtn;
	
	}

    /*!
        reverses the horizontal directions of a motion if the direction of the fighter is different to the default direction in the motion 
        \param _rawDirection the direction of the input from the controller
        \param _defaultDirection the fighter direction from the motion. if this does not match the current fighter direction, the horizontal direction requirements are reversed (e.g. left becomes right)
    */
	Directions ConvertDirection(Directions _rawDirection, FighterDirection _defaultDirection)
	{
		if (CurrentDirection == _defaultDirection)
		{
			return _rawDirection;
		}
		else
		{
			switch (_rawDirection)
			{
				case Directions.LEFTUP:
				{
					return Directions.RIGHTUP;
				}
				case Directions.LEFT:
				{
					return Directions.RIGHT;
				}
				case Directions.LEFTDOWN:
				{
					return Directions.RIGHTDOWN;
				}
				case Directions.RIGHTUP:
				{
					return Directions.LEFTUP;
				}
				case Directions.RIGHT:
				{
					return Directions.LEFT;	
				}
				case Directions.RIGHTDOWN:
				{
					return Directions.LEFTDOWN;
				}
				default:
				{
					return _rawDirection;
				}
			}
		}
	
	}




}



[Serializable]
/*!
    A special move for a fighter, a list of this datatype are contained within the Fighter class
 */
public class SpecialMove
{
	public string Name;//!< The name of the move
	public FightingMotion Motion;//!< The motion required to execute the move
	public Buttons Button;//!< the button that must be pressed to execute the move
	[Range(0.1f, 0.5f)]
	public float InputSpeed;//!< the length of time the player has to enter the move, the bigger the number the more time the player has
	public AnimationParameter AnimParameter;//!< the animation parameter that will be triggered after the move has been entered correctly

}

[Serializable]
/*!
    A container for the hitboxes from the hitbox editor.
 */
public class boundingBoxes
{
	public List<HitboxData> Hitboxes;
	public List<HitboxData> Hurtboxes;
    public string Name;
}

[Serializable]
/*!
    A class for an individual hitobx/hurtbox from the hitbox editor, is converted into a boxcollider2D
*/
public class HitboxData
{
	public float x, y, w, h; 
}

[Serializable]
/*!
    A class representing an animation parameter. used to interface with Unity's animation controller.
 */
public struct AnimationParameter
{
	public string name;
	public AnimationParameterDatatype paramDatatype;
}
