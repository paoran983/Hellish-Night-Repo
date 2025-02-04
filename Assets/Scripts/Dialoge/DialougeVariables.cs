
using System.Collections;
using System.Collections.Generic;
using UnityEngine;using Ink.Runtime;
using System;

public class DialougeVariables
{
    [SerializeField] Dictionary<string, Ink.Runtime.Object> variables;
    private Story globalVarStory;
    public DialougeVariables(TextAsset globalsText) {
        globalVarStory = new Story(globalsText.text);

        // initlaize variables Dic
        variables = new Dictionary<string, Ink.Runtime.Object>();
        foreach(String name in globalVarStory.variablesState) {
            Ink.Runtime.Object val = globalVarStory.variablesState.GetVariableWithName(name);
            variables.Add(name, val);
        }
    }
    public void StartListening(Story story) {
        // persists the var datta before changing data
        VariableToStory(story);
        story.variablesState.variableChangedEvent += HandleVariableChanged;
    }

    public void StopListening(Story story) {
        story.variablesState.variableChangedEvent -= HandleVariableChanged;
    }

    private void HandleVariableChanged(string name, Ink.Runtime.Object val) {
        if(variables.ContainsKey(name)) {
            variables.Remove(name);
            variables.Add(name, val);
        }
    }

    private void VariableToStory(Story story) {
        // goes through each global var
        foreach(KeyValuePair<string,Ink.Runtime.Object> varible in variables) {
            // updates vars in curent story to match varibles dic
            story.variablesState.SetGlobal(varible.Key, varible.Value);
        }
    }
}
