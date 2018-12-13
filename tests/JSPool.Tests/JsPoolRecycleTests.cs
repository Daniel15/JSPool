using JavaScriptEngineSwitcher.Core;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace JSPool.Tests
{
    public class JsPoolRecycleTests
    {
        [Fact]
        public void ConcurentJsPoolRecyclingSimulationShouldNotThrow()
        {
            var pool = GetEnginePool();

            void enginesConsumer()
            {
                int n = 1000;

                while ((n--) > 0)
                {
                    pool.GetEngine();
                }
            }

            void recycler()
            {
                int n = 1000;

                while ((n--) > 0)
                {
                    pool.Recycle();
                }
            }

            Parallel.Invoke(recycler, enginesConsumer, enginesConsumer, enginesConsumer, enginesConsumer, enginesConsumer, enginesConsumer, recycler);
        }

        public JsPool GetEnginePool()
        {
            var factory = new Mock<IEngineFactoryForMock>();
            factory.Setup(x => x.EngineFactory()).Returns(new Mock<IJsEngine>().Object);
            var config = new JsPoolConfig
            {
                StartEngines = 100,
                EngineFactory = factory.Object.EngineFactory,
            };

            var pool = new JsPool(config);

            return pool;
        }
    }
}
