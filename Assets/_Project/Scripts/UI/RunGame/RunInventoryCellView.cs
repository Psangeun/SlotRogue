using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SlotRogue.UI.RunGame
{
    public sealed class RunInventoryCellView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [FormerlySerializedAs("_tmpText")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [FormerlySerializedAs("_text")]

        private bool _missingReferenceErrorLogged;

        internal Button Button => _button;

        internal Image Icon => _icon;

        internal TMP_Text NameText => _nameText;

        internal TMP_Text DescriptionText => _descriptionText;

        internal void ValidateRequiredReferences()
        {
            bool hasText = _nameText != null ||
                _descriptionText != null;

            if (_missingReferenceErrorLogged ||
                (_icon != null && hasText))
            {
                return;
            }

            _missingReferenceErrorLogged = true;
            Debug.LogError(
                "[RunInventoryCellView] Inventory cell references must be wired in the inspector. " +
                $"Missing: {BuildMissingReferenceSummary()}",
                this);
        }

        private string BuildMissingReferenceSummary()
        {
            var missing = new System.Collections.Generic.List<string>();
            if (_icon == null) missing.Add("Icon");
            if (_nameText == null &&
                _descriptionText == null)
            {
                missing.Add("Name Text or Description Text");
            }

            return missing.Count > 0 ? string.Join(", ", missing) : "None";
        }
    }
}
