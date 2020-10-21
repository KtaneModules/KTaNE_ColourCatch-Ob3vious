using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Rnd = UnityEngine.Random;

public class colourCatchScript : MonoBehaviour
{

	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMNeedyModule Module;
	public KMSelectable[] Buttons;
	public KMColorblindMode CBM;

	private int solution;
	private bool solved;
	private bool active;
	private bool colorblind;

	private KMSelectable.OnInteractHandler Press(int pos)
	{
		return delegate
		{
			if (active)
			{
				Buttons[pos].AddInteractionPunch();
				if (solution == pos)
				{
					Audio.PlaySoundAtTransform("Poggers", Module.transform);
					solved = true;
				}
				OnNeedyDeactivation();
				Module.HandlePass();
			}
			return false;
		};
	}

	void Awake()
	{
		Module.OnNeedyActivation += delegate { OnNeedyActivation(); };
		Module.OnNeedyDeactivation += delegate { OnNeedyDeactivation(); };
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].OnInteract += Press(i);
		}
		colorblind = CBM.ColorblindModeActive;
	}

	void OnNeedyActivation()
	{
		int[] colour = { Rnd.Range(0, 256), Rnd.Range(0, 256), Rnd.Range(0, 256) };
		int[] fakecol = new int[3];
		int index = Rnd.Range(0, 3);
		if (colorblind)
		{
			while ((fakecol[0] - colour[0] < 12 && colour[0] - fakecol[0] < 12) || (fakecol[0] - colour[0] > 20 || colour[0] - fakecol[0] > 20))
			{
				fakecol[0] = Rnd.Range(0, 256);
			}
			for (int i = 0; i < 2; i++)
			{
				colour[i + 1] = colour[0];
				fakecol[i + 1] = fakecol[0];
			}
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				while ((fakecol[i] - colour[i] < 16 && colour[i] - fakecol[i] < 16 && index == i) || (fakecol[i] - colour[i] > 24 || colour[i] - fakecol[i] > 24))
				{
					fakecol[i] = Rnd.Range(0, 256);
				}
			}
		}
		solution = Rnd.Range(0, 5);
		for (int i = 0; i < 5; i++)
		{
			if (i == solution)
			{
				Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(fakecol[0] / 255f, fakecol[1] / 255f, fakecol[2] / 255f);
			}
			else
			{
				Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(colour[0] / 255f, colour[1] / 255f, colour[2] / 255f);
			}
		}
		solved = false;
		active = true;
	}

	void OnNeedyDeactivation()
	{
		active = false;
		if (!solved && !(Bomb.GetSolvedModuleNames().Count() == Bomb.GetSolvableModuleNames().Count()))
		{
			Module.HandleStrike();
		}
		for (int i = 0; i < 5; i++)
		{
			Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode. '!{0} 4' to press the fourth panel in reading order.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "colorblind")
		{
			colorblind = !colorblind;
		}
		else if (command.Length != 1 || !("12345".Contains(command[0])))
		{
			yield return "sendtochaterror Invalid command.";
			yield break;
		}
		else
		{
			for (int i = 0; i < 5; i++)
			{
				if ((i + 1).ToString() == command)
				{
					Buttons[i].OnInteract();
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		while (true)
		{
			while (!active)
			{
				yield return true;
			}
			Buttons[solution].OnInteract();
			yield return true;
		}
	}
}
