using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GamblingAppGUI
{
	public class MenuManager
	{
		public void InstantiateMenu(object FormToInsantiateOn)
		{
			System.Windows.Forms.Form formToInsantiateOn = (System.Windows.Forms.Form)FormToInsantiateOn;
			formToInsantiateOn.Menu = new MainMenu();
			MenuItem shapesItem = new MenuItem("Main");
			formToInsantiateOn.Menu.MenuItems.Add(shapesItem);
			shapesItem.MenuItems.Add("MainMenu", delegate (object sender, EventArgs e) { SwitchScreen(new MainMenuPage()); });
		}

		public void SwitchScreen(Form formToSwitchTo)
		{
			if (formToSwitchTo.Name != Form.ActiveForm.Name)
			{
				Form.ActiveForm.Hide();
				Console.WriteLine("[*] Chaning screen to " + formToSwitchTo.ToString());
				formToSwitchTo.Show();
			}
		}
	}
}
