using System;

namespace AdventureGameWeb.Services
{
    public class UiStateService
    {
        public string? ActiveModal { get; private set; }

        /// <summary>
        /// True when the Landing Page is showing — enables scrollable layout.
        /// False during all gameplay phases — enables fixed-height layout.
        /// </summary>
        public bool IsLandingPage { get; private set; } = true;

        public event Action? OnChange;

        public void OpenModal(string modalName)
        {
            ActiveModal = modalName;
            OnChange?.Invoke();
        }

        public void CloseModal()
        {
            ActiveModal = null;
            OnChange?.Invoke();
        }

        public void SetLandingPage(bool isLanding)
        {
            IsLandingPage = isLanding;
            OnChange?.Invoke();
        }
    }
}
