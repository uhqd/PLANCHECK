﻿#pragma checksum "..\..\..\..\xaml_datas\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "261433EC9E07B937B4BDB3005DAF9C1B903E05A373FC56CC99D8CC62320163D7"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using PlanCheck;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PlanCheck {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 91 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button prefButton;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock defaultProtocol;
        
        #line default
        #line hidden
        
        
        #line 116 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Verif_button;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OK_button;
        
        #line default
        #line hidden
        
        
        #line 120 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button HELP_button;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView CheckList;
        
        #line default
        #line hidden
        
        
        #line 140 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button exportPDF_button;
        
        #line default
        #line hidden
        
        
        #line 141 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button createCheckListWord_button;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PlanCheck_0020_0001.esapi;component/xaml_datas/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\xaml_datas\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.prefButton = ((System.Windows.Controls.Button)(target));
            
            #line 91 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.prefButton.Click += new System.Windows.RoutedEventHandler(this.preferences_button_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.defaultProtocol = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.Verif_button = ((System.Windows.Controls.Button)(target));
            
            #line 116 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.Verif_button.Click += new System.Windows.RoutedEventHandler(this.Choose_file_button_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.OK_button = ((System.Windows.Controls.Button)(target));
            
            #line 118 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.OK_button.Click += new System.Windows.RoutedEventHandler(this.OK_button_click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.HELP_button = ((System.Windows.Controls.Button)(target));
            
            #line 120 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.HELP_button.Click += new System.Windows.RoutedEventHandler(this.Button_Click_help);
            
            #line default
            #line hidden
            return;
            case 6:
            this.CheckList = ((System.Windows.Controls.ListView)(target));
            return;
            case 7:
            this.exportPDF_button = ((System.Windows.Controls.Button)(target));
            
            #line 140 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.exportPDF_button.Click += new System.Windows.RoutedEventHandler(this.exportPDF_button_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.createCheckListWord_button = ((System.Windows.Controls.Button)(target));
            
            #line 141 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.createCheckListWord_button.Click += new System.Windows.RoutedEventHandler(this.createCheckListWord_button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

