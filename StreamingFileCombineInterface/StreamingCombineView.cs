﻿using FileCombiner.Contracts;
using StreamingFileCombineInterface.Contracts;
using StreamingFileCombineInterface.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StreamingFileCombineInterface
{
	/// <summary>
	/// UI for the application.
	/// </summary>
	public partial class StreamingCombineView : Form, IStreamingCombineView
	{
		#region Fields

		private readonly IStreamingCombinePresenter _presenter;

		private List<Tuple<Button, bool>> _buttonsAndInitialStates;

		#endregion Fields

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamingCombineView"/> class.
		/// </summary>
		public StreamingCombineView()
		{
			InitializeComponent();

			_presenter = new StreamingCombineViewPresenter(this);

			bsConversionMetaData.DataSource = new StreamingCombineUiModel();

			btnSetTempChunkFilesLocation.Click += SetTempChunkFilesLocationClick;
			btnSetCombinedFileLocation.Click += SetCombinedFileLocationClick;
			btnSetUnconvertedFileLocation.Click += SetCombinedFileLocationClick;
			btnSetConvertedFileLocation.Click += SetConvertedFilePathClick;

			btnGetChunkFileList.Click += DownloadChunkFileListClick;
			btnDownloadChunkFiles.Click += DownloadChunkFilesClick;
			btnCombineChunkFiles.Click += CombineChunkFilesClick;
			btnConvertFile.Click += ConvertFileClick;

			btnCancelDownloadChunks.Click += CancelDownloadChunksClick;
			btnCancelCombineChunks.Click += CancelCombineChunksClick;

			txtChunkFileUrl.TextChanged += ChunkFileUrlTextChanged;
			txtCombinedFileLocation.TextChanged += ConvertFileTextChanged;
			txtConvertedFileLocation.TextChanged += ConvertFileTextChanged;
			
			_buttonsAndInitialStates = new List<Tuple<Button, bool>>
			{
				new Tuple<Button, bool>(btnGetChunkFileList, false),
				new Tuple<Button, bool>(btnSetTempChunkFilesLocation, true),
				new Tuple<Button, bool>(btnDownloadChunkFiles, false),
				new Tuple<Button, bool>(btnSetCombinedFileLocation, true),
				new Tuple<Button, bool>(btnCombineChunkFiles, true),
				new Tuple<Button, bool>(btnSetUnconvertedFileLocation, true),
				new Tuple<Button, bool>(btnSetConvertedFileLocation, true),
				new Tuple<Button, bool>(btnConvertFile, false)
			};
			
			InitializeControls();
		}
		
		#endregion Constructor
		
		#region Event Handlers

		#region File Set Locations
		
		/// <summary>
		/// Sets the temporary location.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SetTempChunkFilesLocationClick(object sender, EventArgs e)
		{
			StreamingCombineUiModel conversionData = GetBoundMetaData();
			string tempDirectory = conversionData.TempDirectory;

			FolderBrowserDialog dialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true,
			};

			if (Directory.Exists(tempDirectory))
			{
				dialog.SelectedPath = tempDirectory;
			}
			else
			{
				dialog.RootFolder = Environment.SpecialFolder.Desktop;
			}

			dialog.ShowDialog();

			conversionData.TempDirectory = dialog.SelectedPath;
		}

		/// <summary>
		/// Sets the combined file location click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		void SetCombinedFileLocationClick(object sender, EventArgs e)
		{
			StreamingCombineUiModel conversionData = GetBoundMetaData();

			OpenFileDialog dialog = new OpenFileDialog
			{
				AddExtension = true,
				DefaultExt = ".ts",
				Filter = "MPEG Transport Stream File (*.ts)|*.ts"
			};

			dialog.ShowDialog();

			conversionData.UnconvertedFilePath = dialog.FileName;

			SetSuggestedControl(btnSetConvertedFileLocation, true);
		}

		/// <summary>
		/// Sets the converted file path.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SetConvertedFilePathClick(object sender, EventArgs e)
		{
			StreamingCombineUiModel conversionData = GetBoundMetaData();

			SaveFileDialog dialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = ".mp4",
				Filter = "MP4 File (*.mp4)|*.mp4|MKV File (*.mkv)|*.mkv"
			};

			dialog.ShowDialog();

			SetSuggestedControl(btnConvertFile, false);

			conversionData.ConvertedFilePath = dialog.FileName;
		}

		#endregion File Set Locations

		#region Action Buttons

		/// <summary>
		/// Downloads the chunk file list.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private async void DownloadChunkFileListClick(object sender, EventArgs e)
		{
			IProgress<IProgressData> progressIndicator = new Progress<IProgressData>(ReportChunkFileListProgress);

			await _presenter.GetChunkFileList(GetBoundMetaData(), progressIndicator);

			SetSuggestedControl(btnDownloadChunkFiles, true);
		}

		/// <summary>
		/// Downloads the chunk files.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private async void DownloadChunkFilesClick(object sender, EventArgs e)
		{
			Progress<IProgressData> progressIndicator = new Progress<IProgressData>(ReportDownloadChunkFilesProgress);

			await _presenter.DownloadChunkFiles(GetBoundMetaData(), progressIndicator);

			SetSuggestedControl(btnCombineChunkFiles, true);
		}

		/// <summary>
		/// Combines the chunks files.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		void CombineChunkFilesClick(object sender, EventArgs e)
		{
			Progress<IProgressData> progressIndicator = new Progress<IProgressData>(ReportCombineFileProgress);

			_presenter.CombineChunkFiles(GetBoundMetaData(), progressIndicator);

			SetSuggestedControl(btnSetConvertedFileLocation, true);
		}
		
		/// <summary>
		/// Converts the file.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ConvertFileClick(object sender, EventArgs e)
		{
			Progress<IProgressData> progressIndicator = new Progress<IProgressData>(ReportFileConversionProgress);

			StreamingCombineUiModel streamingCombineUiModel = GetBoundMetaData();

			_presenter.ConvertFile(streamingCombineUiModel, progressIndicator);
		}

		#endregion Action Buttons

		#region Text Box Related
		
		/// <summary>
		/// The Chunks file URL text has changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ChunkFileUrlTextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(((TextBox)sender).Text))
			{
				SetSuggestedControl(btnGetChunkFileList, true);
			}
			else
			{
				SetSuggestedControl(txtChunkFileUrl, true);
				btnGetChunkFileList.Enabled = false;
			}
		}

		/// <summary>
		/// Enables the 'Convert File' button if both the 'Combined File Location' and 'Converted File Location' are not empty.
		/// </summary>
		/// <param name="sender">Not used.</param>
		/// <param name="e">Not Used.</param>
		private void ConvertFileTextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtCombinedFileLocation.Text)
				&& !string.IsNullOrWhiteSpace(txtConvertedFileLocation.Text))
			{
				btnConvertFile.Enabled = true;
			}
			else
			{
				btnConvertFile.Enabled = false;
			}
		} 

		#endregion Text Box Related

		#region Cancellation Buttons

		/// <summary>
		/// Cancels the download chunks click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		void CancelDownloadChunksClick(object sender, EventArgs e)
		{
			_presenter.CancelDownloadChunks();
		}

		/// <summary>
		/// Cancels the combine chunks click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		void CancelCombineChunksClick(object sender, EventArgs e)
		{
			_presenter.CancelCombineChunks();
		}
		
		#endregion Cancellation Buttons
		
		#endregion Event Handlers

		#region Helper Methods

		/// <summary>
		/// Initializes the controls to their initial starting enabled/disabled state.
		/// </summary>
		private void InitializeControls()
		{
			foreach (Tuple<Button, bool> button in _buttonsAndInitialStates)
			{
				button.Item1.Enabled = button.Item2;
			}

			SetSuggestedControl(txtChunkFileUrl, true);
		}

		/// <summary>
		/// Gets the bound meta data.
		/// </summary>
		/// <returns>The ConversionMetaData object.</returns>
		private StreamingCombineUiModel GetBoundMetaData()
		{
			return bsConversionMetaData.DataSource as StreamingCombineUiModel;
		}

		/// <summary>
		/// Sets the suggested control - by highlighting the particlar control.
		/// </summary>
		/// <param name="buttonToColorize">The button to colorize.</param>
		/// <param name="setControlEnabled">if set to <c>true</c> [set control enabled].</param>
		private void SetSuggestedControl(Control buttonToColorize, bool setControlEnabled)
		{
			ResetBackcolorOfControls(Controls);

			buttonToColorize.BackColor = Color.DarkOrange;
			buttonToColorize.Enabled = setControlEnabled;
		}

		/// <summary>
		/// Resets the backcolor of controls.
		/// </summary>
		/// <param name="controls">The controls.</param>
		private void ResetBackcolorOfControls(IEnumerable controls)
		{
			foreach (object control in controls)
			{
				if (control is GroupBox)
				{
					ResetBackcolorOfControls(((GroupBox)control).Controls);
				}
				else if(control is Control)
				{
					((Control)control).ResetBackColor();
				}
			}
		}

		/// <summary>
		/// Updates the progress bar.
		/// </summary>
		/// <param name="progressBar">The progress bar.</param>
		/// <param name="value">The value.</param>
		private void UpdateProgressBar(ProgressBar progressBar, int value)
		{
			if (value >= 0 || value <= 100)
			{
				progressBar.Value = value;
			}
		}

		#endregion Helper Methods
		
		#region Progress Reporting

		/// <summary>
		/// Reports the chunk file list progress.
		/// </summary>
		/// <param name="value">The value.</param>
		private void ReportChunkFileListProgress(IProgressData value)
		{
			AddToProgressStatus(value.Status);
			UpdateProgressBar(pbChunkFileList, value.PercentDone);
		}

		/// <summary>
		/// Reports the chunk files progress.
		/// </summary>
		/// <param name="value">The value.</param>
		private void ReportDownloadChunkFilesProgress(IProgressData value)
		{
			AddToProgressStatus(value.Status);
			UpdateProgressBar(pbChunkFiles, value.PercentDone);
		}

		/// <summary>
		/// Reports the combine file progress.
		/// </summary>
		/// <param name="value">The value.</param>
		private void ReportCombineFileProgress(IProgressData value)
		{
			AddToProgressStatus(value.Status);
			UpdateProgressBar(pbCombineChunks, value.PercentDone);
		}

		/// <summary>
		/// Reports the file conversion progress.
		/// </summary>
		/// <param name="value">The value.</param>
		private void ReportFileConversionProgress(IProgressData value)
		{
			AddToProgressStatus(value.Status);
			UpdateProgressBar(pbFileConversion, value.PercentDone);
		} 

		#endregion Progress Reporting 
	
		#region IStreamingCombineView Members

		/// <summary>
		/// Adds to progress status.
		/// </summary>
		/// <param name="newestInput">The newest input.</param>
		public void AddToProgressStatus(string newestInput)
		{
			// don't update the status if status field wasn't used
			if (string.IsNullOrWhiteSpace(newestInput))
			{
				return;
			}

			List<string> progressStatusList = txtProgressStatus.Lines.ToList();
			progressStatusList.Add(newestInput);
			progressStatusList.Reverse();

			txtProgressStatus.Lines = progressStatusList.ToArray();
		}

		#endregion IStreamingCombineView Members
	}
}
