using System;

namespace Infra.DataService.Protocol
{
    public interface IDataSender<out TData>
    {
        event Action<TData> SenderDataReady;
    }

    public interface IDataEncapsulator<in TData>
    {
        void Encapsulate(TData data);
    }

    public interface IDataProvider<out TData>
    {
        event Action<TData> ProviderDataReady;
    }

    public interface IDataReceiver<in TData>
    {
        void Receive(TData data);
    }

    public static class DataConnectors
    {
        public static void ConnectToDownstream<T>(this IDataSender<T> sender, IDataEncapsulator<T> encapsulator)
        {
            sender.SenderDataReady += encapsulator.Encapsulate;
        }

        public static void ConnectToUpstream<T>(this IDataProvider<T> provider, IDataReceiver<T> receiver)
        {
            provider.ProviderDataReady += receiver.Receive;
        }
    }
}
