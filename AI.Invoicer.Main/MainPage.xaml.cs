using AI.Invoicer.Infrastructure.AIService;
using System.Threading.Tasks;

namespace AI.Invoicer.Main
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            var response = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.StorageWrite>();

            var files = new PathService().GetContentOfFolder();
            var service1 = new OnnxInferenceService(4096);
            await service1.InitializeAsync(files);
            var service = new InvoiceCommandService(service1);
            
            var reponse = await service.GetCommandsFromPromptAsync("Ajoute une ligne de fret aerien qui vaut 500 dollars et une ligne de main d'oeuvre de 100 dollars et une ligne pour deux serveur de 21000 USD et modifie la ligne des serveur à 15000 USD pour les deux serveurs");

        }
    }
}
