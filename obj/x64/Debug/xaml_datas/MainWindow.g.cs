﻿#pragma checksum "..\..\..\..\xaml_datas\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "57ECCCEB191D29ECE6B0FEFFCBD2B7A55678EAAF18F5D7FEA6FE7DB89F4C98D1"
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
        
        
        #line 92 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock modeName;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox UserMode;
        
        #line default
        #line hidden
        
        
        #line 130 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock defaultProtocol;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Verif_button;
        
        #line default
        #line hidden
        
        
        #line 134 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OK_button;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button HELP_button;
        
        #line default
        #line hidden
        
        
        #line 148 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView CheckList;
        
        #line default
        #line hidden
        
        
        #line 156 "..\..\..\..\xaml_datas\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button exportPDF_button;
        
        #line default
        #line hidden
        
        
        #line 157 "..\..\..\..\xaml_datas\MainWindow.xaml"
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
            System.Uri resourceLocater = new System.Uri("/PlanCheck_Beta_0019_0000.esapi;component/xaml_datas/mainwindow.xaml", System.UriKind.Relative);
            
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
            this.modeName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.UserMode = ((System.Windows.Controls.ComboBox)(target));
            
            #line 100 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.UserMode.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.UserMode_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.defaultProtocol = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.Verif_button = ((System.Windows.Controls.Button)(target));
            
            #line 132 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.Verif_button.Click += new System.Windows.RoutedEventHandler(this.Choose_file_button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.OK_button = ((System.Windows.Controls.Button)(target));
            
            #line 134 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.OK_button.Click += new System.Windows.RoutedEventHandler(this.OK_button_click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.HELP_button = ((System.Windows.Controls.Button)(target));
            
            #line 136 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.HELP_button.Click += new System.Windows.RoutedEventHandler(this.Button_Click_help);
            
            #line default
            #line hidden
            return;
            case 7:
            this.CheckList = ((System.Windows.Controls.ListView)(target));
            return;
            case 8:
            this.exportPDF_button = ((System.Windows.Controls.Button)(target));
            
            #line 156 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.exportPDF_button.Click += new System.Windows.RoutedEventHandler(this.exportPDF_button_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.createCheckListWord_button = ((System.Windows.Controls.Button)(target));
            
            #line 157 "..\..\..\..\xaml_datas\MainWindow.xaml"
            this.createCheckListWord_button.Click += new System.Windows.RoutedEventHandler(this.createCheckListWord_button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

