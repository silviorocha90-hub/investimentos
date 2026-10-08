import {
  StrictMode,
} from 'react'

import {
  createRoot,
} from 'react-dom/client'

import './index.css'

const userAgent = navigator.userAgent
const plataformaMovel =
  /iPad|iPhone|iPod/i.test(userAgent)
    ? 'ios'
    : /Android/i.test(userAgent)
      ? 'android'
      : null

if (plataformaMovel) {
  document.documentElement.classList.add(
    'mobile-device',
    `platform-${plataformaMovel}`,
  )
}

import {
  AuthProvider,
} from './auth/AuthContext'

import {
  AuthGate,
} from './auth/AuthGate'

createRoot(
  document.getElementById(
    'root',
  )!,
).render(
  <StrictMode>
    <AuthProvider>
      <AuthGate />
    </AuthProvider>
  </StrictMode>,
)