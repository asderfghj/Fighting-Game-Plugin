using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fightingMotion", menuName="Fighting Motion")]
/*!
    Used to execute special moves.  
*/
public class FightingMotion : ScriptableObject 
{
	public List<Directions> ManoeuvreDirections;//!< directions needed to execute the move
	public bool ignoreCenterPositions;//!< determines whether centre positions should be ignored or not. Consider if the created motion is going to require putting the joystick back to the centre position.
	public FighterDirection defaultDirection;//!< defines the direction that the fighter is facing when the defined move is executed. If the fighter is facing the opposite direction to the default, the horizontal directions in the move are reversed. 
	
}
