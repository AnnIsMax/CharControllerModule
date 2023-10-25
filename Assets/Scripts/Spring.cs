using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Spring
{

    public static float SpringCalutation( float CurrentDistanceToDefault, 
                                          float TargetDistanceToDefault, 
                                          float CurrentLinearVelocity,
                                          float TargetLinearVelocity,
                                          float SpringStrength,
                                          float SpringDamper)
    {
        float springActionFactor = 0f;

        //Distance Between Current Position and Target Position
        float diffDistance = CurrentDistanceToDefault - TargetDistanceToDefault;

        //Relative Velocity of the Moving Input Object
        float relativeVelocity = CurrentLinearVelocity - TargetLinearVelocity;


        springActionFactor = SpringStrength * diffDistance - relativeVelocity * SpringDamper;




        return springActionFactor;    
    }












}
