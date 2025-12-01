// <copyright file="IUnityPlugin.java"  company="Google LLC">
//
// Copyright 2025 Google LLC
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
// </copyright>
//  ----------------------------------------------------------------------

package com.google.xr.androidxrunitysamples.java;

import android.content.Context;

/** Standard interface for Unity Android plugins managed by the new architecture. */
public interface IUnityPlugin {
  /**
   * Initializes the plugin instance.
   *
   * @param context Android application context.
   * @param callback An implementation of IPluginCallback (usually an AndroidJavaProxy from Unity)
   *     to send asynchronous events back to C#.
   */
  void initialize(Context context, IPluginCallback callback);

  /**
   * Triggers a specific action within the plugin.
   *
   * @param actionName A string identifying the action to perform (e.g., "startRecognition",
   *     "stopTTS").
   * @param jsonArgs A JSON string containing arguments for the action (can be "{}" if no args).
   */
  void callAction(String actionName, String jsonArgs);

  /**
   * Cleans up resources used by the plugin instance. Called when the corresponding C# bridge is
   * disposed.
   */
  void destroy();
}
