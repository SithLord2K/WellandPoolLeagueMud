using MudBlazor;
using System.Threading.Tasks;

namespace WellandPoolLeagueMud.Data.Services
{
    public class DialogService
    {
        private readonly IDialogService _mudDialogService;

        public DialogService(IDialogService mudDialogService)
        {
            _mudDialogService = mudDialogService;
        }

        public async Task<bool> ShowConfirmationDialog(string title, string message, string buttonText = "Delete", Color buttonColor = Color.Error)
        {
            var parameters = new DialogParameters
            {
                ["Title"] = title,
                ["ContentText"] = message,
                ["ButtonText"] = buttonText,
                ["Color"] = buttonColor
            };

            var dialog = await _mudDialogService.ShowAsync<WellandPoolLeagueMud.Components.Dialogs.ConfirmExecutionDialog>(title, parameters);
            var result = await dialog.Result;

            return !result!.Canceled && result.Data is true;
        }
    }
}