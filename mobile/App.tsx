import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { StatusBar } from 'expo-status-bar';
import { AuthProvider } from './src/context/AuthContext';
import { RadarProvider } from './src/context/RadarContext';
import { RootNavigator } from './src/navigation/RootNavigator';
import { colors } from './src/theme';

export default function App() {
  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <AuthProvider>
        <RadarProvider>
          <NavigationContainer>
            <RootNavigator />
          </NavigationContainer>
        </RadarProvider>
      </AuthProvider>
    </SafeAreaProvider>
  );
}
