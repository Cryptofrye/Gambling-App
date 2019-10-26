using System;
using RestSharp;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GamblingAppGUI
{
	public partial class LoginForm : Form
	{
		public MenuManager menuManager = new MenuManager();
		static RestClient client;
		static CookieContainer cookieContainer = new CookieContainer(); //CookieContainer so that all RestRequests use the same cookies (used for authentication with the server)
		static RestRequest request;
		static IRestResponse response;

		public LoginForm()
		{
			InitializeComponent();
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			string sUsername = entryUsername.Text;
			string sPassword = entryPassword.Text;
			if (AttemptLogin(sUsername, sPassword))
			{
				MainMenuPage mainMenuPage = new MainMenuPage();
				menuManager.SwitchScreen(mainMenuPage);
			}
			else
			{
				MessageBox.Show("Invalid Credentials, please try again.", "Gambling App", MessageBoxButtons.OK);
			}
		}

		private bool AttemptLogin(string sUsername, string sPassword)
		{
			client = new RestClient(ServerConfig.ipAddress);
			request = new RestRequest("users/login/");
			request.AddParameter("username", sUsername);
			request.AddParameter("password", sPassword);
			request.AddHeader("ContentType", "application/x-www-form-urlencoded");
			response = client.Post(request);
			if (response.StatusCode == HttpStatusCode.OK)
				return true;
			else if (response.StatusCode == HttpStatusCode.Forbidden)
				return false;
			return false;
		}
	}
}
