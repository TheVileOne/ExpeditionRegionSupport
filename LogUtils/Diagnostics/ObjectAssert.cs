namespace LogUtils.Diagnostics
{
    public readonly struct ObjectAssert
    {
        public readonly AssertArgs _settings;
        private readonly object _target;

        public ObjectAssert(object assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        public bool IsEqualTo(object checkData)
        {
            var result = Assert.IsEqual(_target, checkData);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool DoesNotEqual(object checkData)
        {
            var result = Assert.DoesNotEqual(_target, checkData);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }
    }
}
