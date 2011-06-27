using System;
using System.Collections.Generic;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	ListStore m_assignments = new ListStore (typeof(int), typeof(string));
	Dictionary<int,string> m_redefinitions = new Dictionary<int, string>();

	RenameEEG.Renamer m_renamer;
	
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		
		m_renamer = new RenameEEG.Renamer("",m_redefinitions);
		
		TreeViewColumn indexColumn = new TreeViewColumn ();
		indexColumn.Title = "Index";
		
		CellRendererText indexRenderer = new CellRendererText();
		indexColumn.PackStart(indexRenderer, true);
		
		TreeViewColumn valueColumn = new TreeViewColumn ();
		valueColumn.Title = "Wert";
		
		CellRendererText valueRenderer = new CellRendererText();
		valueColumn.PackStart(valueRenderer, true);
		
		tvNewValues.Model = m_assignments;
		
		tvNewValues.AppendColumn(indexColumn);
		tvNewValues.AppendColumn(valueColumn);
		
		indexColumn.AddAttribute(indexRenderer, "text", 0);
		valueColumn.AddAttribute(valueRenderer, "text", 1);
		
		btnQuit.Clicked += new EventHandler (btnQuit_Clicked);
		btnLoad.Clicked += new EventHandler (btnLoad_Clicked);
		btnAddAssignment.Clicked += new EventHandler (btnAddAssignment_Clicked);
		btnRename.Clicked += new EventHandler (btnRename_Clicked);
		
		m_renamer.Pattern = txtPattern.Text;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	private void btnQuit_Clicked (object sender, EventArgs args)
	{
		Application.Quit();
	}

	private void btnLoad_Clicked (object sender, EventArgs args)
	{
		FileChooserDialog fileDialog = new FileChooserDialog ("Datei Laden", this, FileChooserAction.Open);
		
		fileDialog.TransientFor = this;
		
		fileDialog.AddButton(Gtk.Stock.Cancel, ResponseType.Cancel);
		fileDialog.AddButton(Gtk.Stock.Ok, ResponseType.Ok);
		
		int result = fileDialog.Run();
		fileDialog.Hide();
		
		if(result == (int)ResponseType.Ok)
		{
			txtFilename.Text = fileDialog.Filename;
		}
	}

	private void btnAddAssignment_Clicked (object sender, EventArgs args)
	{
		m_assignments.Foreach(new TreeModelForeachFunc(ForEachObject));
		
		if(!m_redefinitions.ContainsKey((int)sbtnNewValueIndex.Value))
		{
		
			m_assignments.AppendValues((int)sbtnNewValueIndex.Value, txtNewValue.Text);
			sbtnNewValueIndex.Value++;
			txtNewValue.Text = "";
			txtNewValue.GrabFocus();
			
			m_assignments.Foreach(new TreeModelForeachFunc(ForEachObject));
		}
		
		
	}
	
	private void PatternChanged(object sender, EventArgs args)
	{
		m_renamer.Pattern = txtPattern.Text;
	}
		                                               
	private bool ForEachObject(TreeModel model, TreePath path, TreeIter iter)
	{
		m_redefinitions.Clear();
		
		int index = (int)model.GetValue(iter, 0);
		String value = model.GetValue(iter, 1).ToString();
		
		m_redefinitions.Add(index, value);
		
		return false;
	}

	private void btnRename_Clicked (object sender, EventArgs args)
	{
		
		m_renamer.Rename(txtFilename.Text);
	}
	
}

