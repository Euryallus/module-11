using UnityEngine;
using TMPro;
using System.Linq;

// Allows input fields to only accept letters, numbers, spaces, and simple punctuation

[RequireComponent(typeof(TMP_InputField))]
public class AlphanumericPunctuationInputValidator : MonoBehaviour
{
	private static readonly char[] validPunctuation = new char[] { '(', ')', '?', '!', '&', ':', ',', '.' };

	void Awake()
	{
		TMP_InputField input = GetComponent<TMP_InputField>();
		input.onValidateInput = ValidateInput;
	}

	static char ValidateInput(string text, int charIndex, char addedChar)
	{
		if (char.IsLetterOrDigit(addedChar) || addedChar == ' ' || validPunctuation.Contains(addedChar))
		{
			return addedChar;
		}
		return '\0';
	}
}