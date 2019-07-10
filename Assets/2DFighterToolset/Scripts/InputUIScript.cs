using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*!
    Debugging class to see what inputs are being recieved from the controller. 
*/
public class InputUIScript : MonoBehaviour {

    public Sprite RightArrow, LeftArrow, UpArrow, DownArrow, LeftUpArrow, RightUpArrow, LeftDownArrow, RightDownArrow;
	public Sprite LightPunch, MediumPunch, HeavyPunch, LightKick, MediumKick, HeavyKick;
	public List<Image> DirectionImages;
	public List<Image> ButtonImages;
	public Controller MonitoredController;

	void Start()
	{
		MonitoredController.JoystickMoved += UpdateUI;
		UpdateUI ();
	}

	void UpdateUI()
	{
		int PositionCounter = 0;
		for (int i = 0; i < DirectionImages.Count; i++) 
		{
			if (MonitoredController.RecentInputs.Count > PositionCounter)
			{
				bool positionFound = false;
				while (!positionFound)
				{
					if (MonitoredController.RecentInputs.Count > PositionCounter)
					{
						if (MonitoredController.RecentInputs [PositionCounter].direction != Directions.CENTRED)
						{
							DirectionImages [i].sprite = DetermineDirectionSprite (MonitoredController.RecentInputs [PositionCounter].direction);
							DirectionImages [i].color = new Color (1, 1, 1, 1);
							positionFound = true;
						}
						else
						{
							DirectionImages [i].color = new Color (1, 1, 1, 0);
						}

						if (MonitoredController.RecentInputs [PositionCounter].button != Buttons.NULL)
						{
							ButtonImages [i].sprite = DetermineButtonSprite (MonitoredController.RecentInputs [PositionCounter].button);
							ButtonImages [i].color = new Color (1, 1, 1, 1);
							positionFound = true;
						}
						else
						{
							ButtonImages [i].color = new Color (1, 1, 1, 0);
						}

						if(!positionFound)
						{
							PositionCounter++;
						}
					} 

					else
					{
						DirectionImages [i].color = new Color (1, 1, 1, 0);
						ButtonImages [i].color = new Color (1, 1, 1, 0);
						positionFound = true;
					}
				}

				PositionCounter++;
			} 

			else
			{
				DirectionImages [i].color = new Color (1, 1, 1, 0);
				ButtonImages [i].color = new Color (1, 1, 1, 0);
			}
		}

	}



	Sprite DetermineDirectionSprite(Directions _direction)
	{
		switch (_direction) 
		{
		case Directions.LEFT:
			{
				return LeftArrow;
			}
		case Directions.LEFTDOWN:
			{
				return LeftDownArrow;
			}
		case Directions.DOWN:
			{
				return DownArrow;
			}
		case Directions.RIGHTDOWN:
			{
				return RightDownArrow;
			}
		case Directions.RIGHT:
			{
				return RightArrow;
			}
		case Directions.RIGHTUP:
			{
				return RightUpArrow;
			}
		case Directions.UP:
			{
				return UpArrow;
			}
		case Directions.LEFTUP:
			{
				return LeftUpArrow;
			}
		default:
			{
				return null;
			}
		}
	}

	Sprite DetermineButtonSprite(Buttons _button)
	{
		switch (_button) 
		{
		case Buttons.LIGHTPUNCH:
			{
				return LightPunch;
			}
		case Buttons.MEDIUMPUNCH:
			{
				return MediumPunch;
			}
		case Buttons.HEAVYPUNCH:
			{
				return HeavyPunch;
			}
		case Buttons.LIGHTKICK:
			{
				return LightKick;
			}
		case Buttons.MEDIUMKICK:
			{
				return MediumKick;
			}
		case Buttons.HEAVYKICK:
			{
				return HeavyKick;
			}
		default:
			{
				return null;
			}
		}
	}



}
