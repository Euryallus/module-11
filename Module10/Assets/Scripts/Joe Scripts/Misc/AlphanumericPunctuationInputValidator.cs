using UnityEngine;
using TMPro;
using System.Linq;

// ||=======================================================================||
// || AlphanumericPunctuationInputValidator: When used with an input field, ||
// ||   restricts any characters except for letters, numbers, spaces and    ||
// ||   simple punctuation from being entered.							    ||
// ||=======================================================================||
// || Used with various input fields.										||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

[RequireComponent(typeof(TMP_InputField))]
public class AlphanumericPunctuationInputValidator : MonoBehaviour
{
	// All valid punctuation characters that can be entered
	private static readonly char[] validPunctuation = new char[] { '(', ')', '?', '!', '&', ':', ',', '.' };

	void Awake()
	{
		TMP_InputField input = GetComponent<TMP_InputField>();

		// Call ValidateInput when the input field validates input
		input.onValidateInput = ValidateInput;
	}

	static char ValidateInput(string text, int charIndex, char addedChar)
	{
		if (char.IsLetterOrDigit(addedChar) || addedChar == ' ' || validPunctuation.Contains(addedChar))
		{
			// Allow the added character if it's a letter, digit, space or any character listed in the validPunctuation array
			return addedChar;
		}

		// Otherwise do not allow the added character, return the string termination character instead
		return '\0';
	}
}