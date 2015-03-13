/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */
namespace MarrySocket.MExtra.Serialization
{
    using MarrySocket.MExtra.Logging;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        byte[] Serialize(object myClass, Logger logger);

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        object Deserialize(byte[] data, Logger logger);
    }
}
