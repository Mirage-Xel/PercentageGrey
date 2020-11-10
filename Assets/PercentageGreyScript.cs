using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class PercentageGreyScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
	public KMBombModule Module;
	
	public KMSelectable Component;
	public MeshRenderer Backlight;
	public Material[] GreyLevel;
	public Material ModuleBacking;
	public TextMesh PercentLevel;
	
	string Baseline = "";
	string Answer = "";
	bool Animating = false;
	int Common = 0;
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
		Component.OnInteract += delegate () {PercentageCount(); return false; };
		Component.OnInteractEnded += delegate () {Inspection();};
	}

	void Start()
	{
		int Basis = UnityEngine.Random.Range(0,11);
		Backlight.material = GreyLevel[Basis];
		Baseline = Basis != 0 ? Basis.ToString() + "0%" : "0%";
		Debug.LogFormat("[% Grey #{0}] The module's current grey level: {1}", moduleId, Baseline);
	}
	
	IEnumerator PercentageCountCoroutine()
	{
		if (!ModuleSolved && !Animating)
		{
			while (Common != 10)
			{
				yield return new WaitForSecondsRealtime(0.4f);
				Common += 1;
				PercentLevel.text = Common.ToString() + "0%";
			}
		}
	}
	
	void PercentageCount()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		Component.AddInteractionPunch(.2f);
		if (!ModuleSolved && !Animating)
		{
			StartCoroutine(PercentageCountCoroutine());
		}
	}
	
	void Inspection()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
		if (!ModuleSolved && !Animating)
		{
			StopAllCoroutines();
			StartCoroutine(InspectionCoroutine());
		}
	}
	
	IEnumerator InspectionCoroutine()
	{
		if (!ModuleSolved && !Animating)
		{
			Answer = PercentLevel.text;
			Debug.LogFormat("[% Grey #{0}] Module was release during: {1}", moduleId, Answer);
			Animating = true;
			yield return new WaitForSecondsRealtime(0.5f);
			if (Answer == Baseline)
			{
				Debug.LogFormat("[% Grey #{0}] That is correct", moduleId);
				PercentLevel.text = "WOW";
				yield return new WaitForSecondsRealtime(0.5f);
				PercentLevel.text = "COOL";
				yield return new WaitForSecondsRealtime(0.5f);
				PercentLevel.text = "YOU";
				yield return new WaitForSecondsRealtime(0.125f);
				PercentLevel.text = "DID";
				yield return new WaitForSecondsRealtime(0.125f);
				PercentLevel.text = "IT";
				yield return new WaitForSecondsRealtime(0.6f);
				Module.HandlePass();
				Backlight.material = ModuleBacking;
				PercentLevel.text = "";
				ModuleSolved = true;
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			}
			
			else
			{
				Debug.LogFormat("[% Grey #{0}] That is incorrect", moduleId);
				Module.HandleStrike();
				PercentLevel.text = "0%";
				Start();
				Animating = false;
				Common = 0;
			}
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To release the button on a certain percentage, use !{0} release [0%-100%]";
    #pragma warning restore 414
	
	string[] ValidPercentage = {"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};

    IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split();
		if (Regex.IsMatch(parameters[0], @"^\s*release\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. Command was ignored";
				yield break;
			}
			
			if (!parameters[1].EqualsAny(ValidPercentage))
			{
				yield return "sendtochaterror Percentage being submitted is not valid. Command was ignored";
				yield break;
			}
			Component.OnInteract();
			while (PercentLevel.text != parameters[1])
			{
				yield return null;
			}
			Component.OnInteractEnded();
		}
	}
}
