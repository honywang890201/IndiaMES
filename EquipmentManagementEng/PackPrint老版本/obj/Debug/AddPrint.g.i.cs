﻿#pragma checksum "..\..\AddPrint.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "44321BD6DF12E7CB6D380E6A414E8A330781A1A5"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using Component.Controls.User.GenerateControl;
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


namespace PackPrint {
    
    
    /// <summary>
    /// AddPrint
    /// </summary>
    public partial class AddPrint : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\AddPrint.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbType;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\AddPrint.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblScan;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\AddPrint.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.Controls.User.GenerateControl.TextBox txtScan;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\AddPrint.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock lblDescript;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\AddPrint.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.Controls.User.GenerateControl.TextBox txtDescript;
        
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
            System.Uri resourceLocater = new System.Uri("/PackPrint_N;component/addprint.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AddPrint.xaml"
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
            
            #line 5 "..\..\AddPrint.xaml"
            ((PackPrint.AddPrint)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cmbType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 20 "..\..\AddPrint.xaml"
            this.cmbType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbType_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.lblScan = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.txtScan = ((Component.Controls.User.GenerateControl.TextBox)(target));
            
            #line 24 "..\..\AddPrint.xaml"
            this.txtScan.KeyDown += new System.Windows.Input.KeyEventHandler(this.txtScan_KeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.lblDescript = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.txtDescript = ((Component.Controls.User.GenerateControl.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

