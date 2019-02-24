
namespace Infra.DataService.Protocol.UnitTest
{
    public static class Helper
    {
        public static void AttachErrorHandler(this ProtocolTree tree)
        {
            tree.DataIntegrityError += err => throw err;
            tree.DataSymmetricityError += err => throw err;
        }


        public static void AttachErrorHandler(this DataSerializer s)
        {
            s.DataIntegrityError += err => throw err;
            s.DataSymmetricityError += err => throw err;
        }
    }
}
