#region License
// Copyright 2008-2010 Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://FluentValidation.codeplex.com
#endregion

namespace FluentValidation.Internal {
	using System;
	using System.Collections.Generic;
	using System.Threading;

	/// <summary>
	/// Thread safe instance cache.
	/// </summary>
	public class InstanceCache {
		readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
#if SILVERLIGHT
		readonly object locker = new object();
#else
		readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
#endif
		public object GetOrCreateInstance(Type type) {
			return GetOrCreateInstance(type, Activator.CreateInstance);
		}

		public object GetOrCreateInstance(Type type, Func<Type, object> factory) {
#if SILVERLIGHT
			object existingInstance;
			if(cache.TryGetValue(type, out existingInstance)) {
				return existingInstance;
			}

			lock(locker) {
				if (cache.TryGetValue(type, out existingInstance)) {
					return existingInstance;
				} 
				else {
					var newInstance = factory(type);
					cache[type] = newInstance;
					return newInstance;
				}
			}
#else
			// If the item is already in the cache, get it out.
			locker.EnterReadLock();
			try {
				object existingInstance;
				if (cache.TryGetValue(type, out existingInstance)) {
					return existingInstance;
				}
			}
			finally {
				locker.ExitReadLock();
			}

			// If it is not in the cache, try and insert it.
			// However, it may have been inserted by another thread, in which case 
			// we should retrieve that instance instead.

			locker.EnterWriteLock();
			try {
				object existingInstance;
				if (cache.TryGetValue(type, out existingInstance)) {
					return existingInstance;
				}

				var newInstance = factory(type);
				cache[type] = newInstance;
				return newInstance;
			}
			finally {
				locker.ExitWriteLock();
			}
#endif
		}
	}
}