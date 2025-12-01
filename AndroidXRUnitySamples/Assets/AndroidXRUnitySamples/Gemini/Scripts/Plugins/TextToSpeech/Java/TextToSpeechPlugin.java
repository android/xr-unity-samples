// <copyright file="TextToSpeechPlugin.java"  company="Google LLC">
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
import android.os.Bundle;
import android.speech.tts.TextToSpeech;
import android.speech.tts.UtteranceProgressListener;
import android.util.Log;
import java.util.Locale;
import java.util.UUID;
import org.json.JSONException;
import org.json.JSONObject;

/** Implements TTS functionality for Unity objects. */
public class TextToSpeechPlugin implements IUnityPlugin, TextToSpeech.OnInitListener {

  private static final String TAG = "TTSPlugin";

  private static final String ACTION_SPEAK = "speak";
  private static final String ACTION_STOP = "stop";
  private static final String ACTION_SET_LANG = "setLanguage";
  private static final String ACTION_SET_PITCH = "setPitch";
  private static final String ACTION_SET_RATE = "setSpeechRate";

  private static final String EVENT_INIT_SUCCESS = "TTS_InitSuccess";
  private static final String EVENT_INIT_ERROR = "TTS_InitError";
  private static final String EVENT_SPEAK_START = "TTS_SpeakStart";
  private static final String EVENT_SPEAK_DONE = "TTS_SpeakDone";
  private static final String EVENT_SPEAK_ERROR = "TTS_SpeakError";
  private static final String EVENT_LANG_CHANGED = "TTS_LangChanged";
  private static final String EVENT_PITCH_CHANGED = "TTS_PitchChanged";
  private static final String EVENT_RATE_CHANGED = "TTS_RateChanged";

  private Context context;
  private IPluginCallback eventCallback;
  private TextToSpeech tts;
  private boolean isInitialized = false;
  private Locale currentLocale = Locale.getDefault();
  private float currentPitch = 1.0f;
  private float currentRate = 1.0f;

  @Override
  public void initialize(Context context, IPluginCallback callback) {
    this.context = context;
    this.eventCallback = callback;
    Log.d(TAG, "Initializing TextToSpeech engine...");
    tts = new TextToSpeech(context, this);
  }

  @Override
  public void onInit(int status) {
    if (status == TextToSpeech.SUCCESS) {
      isInitialized = true;
      Log.i(TAG, "TextToSpeech Engine Initialized successfully.");

      int langResult = tts.setLanguage(currentLocale);
      if (langResult == TextToSpeech.LANG_MISSING_DATA
          || langResult == TextToSpeech.LANG_NOT_SUPPORTED) {
        Log.w(TAG, "Default language (" + currentLocale + ") not supported or missing data.");
        sendInitResult(true, "Initialized, but default language may not be fully supported.");
      } else {
        Log.d(TAG, "Default language set to: " + currentLocale);
        sendInitResult(true, "Initialized successfully.");
      }

      tts.setPitch(currentPitch);
      tts.setSpeechRate(currentRate);

      tts.setOnUtteranceProgressListener(
          new UtteranceProgressListener() {
            @Override
            public void onStart(String utteranceId) {
              Log.d(TAG, "Utterance started: " + utteranceId);
              sendUtteranceEvent(EVENT_SPEAK_START, utteranceId, null);
            }

            @Override
            public void onDone(String utteranceId) {
              Log.d(TAG, "Utterance done: " + utteranceId);
              sendUtteranceEvent(EVENT_SPEAK_DONE, utteranceId, null);
            }

            @Override
            public void onError(String utteranceId) {
              Log.e(TAG, "Utterance error (deprecated): " + utteranceId);
              sendUtteranceEvent(
                  EVENT_SPEAK_ERROR, utteranceId, "Deprecated onError callback triggered.");
            }

            @Override
            public void onError(String utteranceId, int errorCode) {
              Log.e(TAG, "Utterance error: " + utteranceId + ", Code: " + errorCode);
              sendUtteranceEvent(EVENT_SPEAK_ERROR, utteranceId, "TTS Error Code: " + errorCode);
            }
          });

    } else {
      isInitialized = false;
      Log.e(TAG, "TextToSpeech Engine Initialization Failed!");
      sendInitResult(false, "TTS Engine initialization failed with status: " + status);
      tts = null;
    }
  }

  @Override
  public void callAction(String actionName, String jsonArgs) {
    Log.d(TAG, "callAction received: " + actionName + " with args: " + jsonArgs);
    if (!isInitialized || tts == null) {
      Log.e(TAG, "TTS not initialized, cannot perform action: " + actionName);
      sendErrorEvent("TTS not initialized", actionName);
      return;
    }

    try {
      JSONObject args = new JSONObject(jsonArgs);

      switch (actionName) {
        case ACTION_SPEAK:
          String text = args.getString("text");
          String utteranceId = UUID.randomUUID().toString();
          Bundle params = new Bundle();
          params.putString(TextToSpeech.Engine.KEY_PARAM_UTTERANCE_ID, utteranceId);
          Log.d(TAG, "Speaking text (ID: " + utteranceId + "): " + text);
          tts.speak(text, TextToSpeech.QUEUE_ADD, params, utteranceId);
          break;

        case ACTION_STOP:
          Log.d(TAG, "Stopping speech.");
          tts.stop();
          break;

        case ACTION_SET_LANG:
          String langTag = args.getString("language");
          Locale locale = Locale.forLanguageTag(langTag);
          int result = tts.setLanguage(locale);
          if (result == TextToSpeech.LANG_MISSING_DATA
              || result == TextToSpeech.LANG_NOT_SUPPORTED) {
            Log.w(TAG, "Language " + langTag + " not supported or missing data.");
            sendErrorEvent("Language " + langTag + " not supported or missing data.", actionName);
          } else {
            Log.i(TAG, "Language changed to: " + langTag);
            currentLocale = locale;
            sendSimpleStatusEvent(EVENT_LANG_CHANGED, "Language set to " + langTag);
          }
          break;

        case ACTION_SET_PITCH:
          float pitch = (float) args.getDouble("pitch");
          if (pitch < 0.1f) {
            pitch = 0.1f;
          }
          if (pitch > 2.0f) {
            pitch = 2.0f;
          }
          tts.setPitch(pitch);
          currentPitch = pitch;
          Log.i(TAG, "Pitch changed to: " + pitch);
          sendSimpleStatusEvent(EVENT_PITCH_CHANGED, "Pitch set to " + pitch);
          break;

        case ACTION_SET_RATE:
          float rate = (float) args.getDouble("rate");
          if (rate < 0.1f) {
            rate = 0.1f;
          }
          if (rate > 2.0f) {
            rate = 2.0f;
          }
          tts.setSpeechRate(rate);
          currentRate = rate;
          Log.i(TAG, "Speech rate changed to: " + rate);
          sendSimpleStatusEvent(EVENT_RATE_CHANGED, "Rate set to " + rate);
          break;

        default:
          Log.w(TAG, "Unknown action requested: " + actionName);
          sendErrorEvent("Unknown TTS action: " + actionName, actionName);
          break;
      }

    } catch (JSONException e) {
      Log.e(TAG, "JSON parsing error for action " + actionName + ": " + e.getMessage());
      sendErrorEvent(
          "Invalid arguments for action " + actionName + ": " + e.getMessage(), actionName);
    } catch (Exception e) {
      Log.e(TAG, "Error performing action " + actionName + ": " + e.getMessage(), e);
      sendErrorEvent("Error performing action " + actionName + ": " + e.getMessage(), actionName);
    }
  }

  @Override
  public void destroy() {
    Log.d(TAG, "Destroying TextToSpeechPluginImpl...");
    if (tts != null) {
      Log.d(TAG, "Stopping and shutting down TTS engine.");
      tts.stop();
      tts.shutdown();
      isInitialized = false;
      tts = null;
    }
    this.context = null;
    this.eventCallback = null;
    Log.d(TAG, "TextToSpeechPluginImpl destroyed.");
  }

  private void sendEvent(String jsonPayload) {
    if (eventCallback != null) {
      Log.d(TAG, "Sending event: " + jsonPayload);
      try {
        eventCallback.OnEvent(jsonPayload);
      } catch (Exception e) {
        Log.e(TAG, "Exception sending event to Unity: " + e.getMessage());
      }
    } else {
      Log.w(TAG, "Cannot send event, callback is null.");
    }
  }

  private void sendInitResult(boolean success, String message) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", success ? EVENT_INIT_SUCCESS : EVENT_INIT_ERROR);
      data.put("Message", message);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating init result event: " + e.getMessage());
    }
  }

  private void sendUtteranceEvent(String eventName, String utteranceId, String errorMessage) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", eventName);
      data.put("UtteranceId", utteranceId);
      if (errorMessage != null) {
        data.put("Error", errorMessage);
      }
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating utterance event (" + eventName + "): " + e.getMessage());
    }
  }

  private void sendErrorEvent(String errorMessage, String failedAction) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_SPEAK_ERROR);

      data.put("Error", errorMessage);
      data.put("FailedAction", failedAction);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating error event: " + e.getMessage());
    }
  }

  private void sendSimpleStatusEvent(String eventName, String message) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", eventName);
      data.put("Message", message);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating status event (" + eventName + "): " + e.getMessage());
    }
  }
}
