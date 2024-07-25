using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    private UnityEventDrawer eventDrawer;
    private UnityEventDrawer EventDrawer
    {
        get
        {
            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }
            return eventDrawer;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get attribute data
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        // Is property enabled ?
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        bool wasEnabled = GUI.enabled;

        GUI.enabled = enabled;

        if (!condHAtt.HideInInspector || enabled)
        {
            if (property.type == "UnityEvent")
            {
                EventDrawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (!condHAtt.HideInInspector || enabled)
        {
            if (property.type == "UnityEvent")
            {
                return EventDrawer.GetPropertyHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            //The property is not being drawn
            //We want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;
            //return 0.0f;
        }


        /*
        //Get the base height when not expanded
        var height = base.GetPropertyHeight(property, label);

        // if the property is expanded go through all its children and get their height
        if (property.isExpanded)
        {
            var propEnum = property.GetEnumerator();
            while (propEnum.MoveNext())
                height += EditorGUI.GetPropertyHeight((SerializedProperty)propEnum.Current, GUIContent.none, true);
        }
        return height;*/
    }

    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = !condHAtt.UseOrLogic;

        //Handle property
        SerializedProperty sourcePropertyValue = null;

        //Get the full relative property path of the sourcefield so we can have nested hiding.Use old method when dealing with arrays
        if (!property.isArray)
        {
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
            sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            //if the find failed->fall back to the old system
            if (sourcePropertyValue == null)
            {
                //original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
            }
        }
        else
        {
            //original implementation (doesn't work with nested serializedObjects)
            sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
        }


        if (sourcePropertyValue != null)
        {
            enabled = CheckPropertyType(sourcePropertyValue);
        }
        else
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }

        #region multiple conditions

        //Handle the unlimited property array
        string[] conditionalSourceFieldArray = condHAtt.ConditionalSourceFields;
        bool[] conditionalSourceFieldInverseArray = condHAtt.ConditionalSourceFieldInverseBools;

        for (int index = 0; index < conditionalSourceFieldArray.Length; ++index)
        {
            SerializedProperty sourcePropertyValueFromArray = null;
            if (!property.isArray)
            {
                string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
                string conditionPath = propertyPath.Replace(property.name, conditionalSourceFieldArray[index]); //changes the path to the conditionalsource property path
                sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionPath);

                //if the find failed->fall back to the old system
                if (sourcePropertyValueFromArray == null)
                {
                    //original implementation (doens't work with nested serializedObjects)
                    sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionalSourceFieldArray[index]);
                }
            }
            else
            {
                // original implementation(doens't work with nested serializedObjects) 
                sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionalSourceFieldArray[index]);
            }

            //Combine the results
            if (sourcePropertyValueFromArray != null)
            {
                bool propertyEnabled = CheckPropertyType(sourcePropertyValueFromArray);
                if (conditionalSourceFieldInverseArray.Length >= (index + 1) && conditionalSourceFieldInverseArray[index]) propertyEnabled = !propertyEnabled;

                if (condHAtt.UseOrLogic)
                    enabled = enabled || propertyEnabled;
                else
                    enabled = enabled && propertyEnabled;
            }
            else
            {
                Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
            }
        }

        #endregion


        // Wrap it all up
        if (condHAtt.Inverse) enabled = !enabled;

        return enabled;
    }

    private bool CheckPropertyType(SerializedProperty sourcePropertyValue)
    {
        //Note: add others for custom handling if desired
        switch (sourcePropertyValue.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return sourcePropertyValue.boolValue;
            case SerializedPropertyType.ObjectReference:
                return sourcePropertyValue.objectReferenceValue != null;
            default:
                Debug.LogError("Data type of the property used for conditional hiding [" + sourcePropertyValue.propertyType + "] is currently not supported");
                return true;
        }
    }
}
