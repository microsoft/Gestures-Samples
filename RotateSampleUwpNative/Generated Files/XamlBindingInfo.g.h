﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#pragma once

namespace XamlBindingInfo
{
    ref class XamlBindings;

    class IXamlBindings
    {
    public:
        virtual ~IXamlBindings() {};
        virtual bool IsInitialized() = 0;
        virtual void Update() = 0;
        virtual bool SetDataRoot(::Platform::Object^ data) = 0;
        virtual void StopTracking() = 0;
        virtual void Connect(int connectionId, ::Platform::Object^ target) = 0;
        virtual void ResetTemplate() = 0;
        virtual int ProcessBindings(::Windows::UI::Xaml::Controls::ContainerContentChangingEventArgs^ args) = 0;
        virtual void SubscribeForDataContextChanged(::Windows::UI::Xaml::FrameworkElement^ object, ::XamlBindingInfo::XamlBindings^ handler) = 0;
        virtual void DisconnectUnloadedObject(int connectionId) = 0;
    };

    ref class XamlBindings sealed : 
        ::Windows::UI::Xaml::IDataTemplateExtension, 
        ::Windows::UI::Xaml::Markup::IComponentConnector
    {
    internal:
        XamlBindings(::XamlBindingInfo::IXamlBindings* pBindings);
        void Initialize();
        void Update();
        void StopTracking();
        void Loading(::Windows::UI::Xaml::FrameworkElement^ src, ::Platform::Object^ data);
        void DataContextChanged(::Windows::UI::Xaml::FrameworkElement^ sender, ::Windows::UI::Xaml::DataContextChangedEventArgs^ args);
        void SubscribeForDataContextChanged(::Windows::UI::Xaml::FrameworkElement^ object);

    public:
        // IComponentConnector
        virtual void Connect(int connectionId, ::Platform::Object^ target);

        // IDataTemplateExtension
        virtual bool ProcessBinding(unsigned int);
        virtual int ProcessBindings(::Windows::UI::Xaml::Controls::ContainerContentChangingEventArgs^ args);
        virtual void ResetTemplate();

        virtual void DisconnectUnloadedObject(int connectionId);
    private:
        ~XamlBindings();

        IXamlBindings* _pBindings = nullptr;
    };
}

