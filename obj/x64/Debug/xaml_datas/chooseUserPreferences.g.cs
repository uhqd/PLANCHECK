﻿#pragma checksum "..\..\..\..\xaml_datas\chooseUserPreferences.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "9C6ED0A49C0D198224E4DA78C6448A43E6AD02F39360C6B45A5C1A09DE2EAA30"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using PlanCheck.xaml_datas;
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


namespace PlanCheck.xaml_datas {
    
    
    /// <summary>
    /// chooseUserPreferences
    /// </summary>
    public partial class chooseUserPreferences : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl itemsControl;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button tous;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button aucun;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button close;
        
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
            System.Uri resourceLocater = new System.Uri("/PlanCheck_0022_0034.esapi;component/xaml_datas/chooseuserpreferences.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
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
            this.itemsControl = ((System.Windows.Controls.ItemsControl)(target));
            return;
            case 2:
            this.tous = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
            this.tous.Click += new System.Windows.RoutedEventHandler(this.tous_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.aucun = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
            this.aucun.Click += new System.Windows.RoutedEventHandler(this.aucun_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.close = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\..\xaml_datas\chooseUserPreferences.xaml"
            this.close.Click += new System.Windows.RoutedEventHandler(this.close_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

