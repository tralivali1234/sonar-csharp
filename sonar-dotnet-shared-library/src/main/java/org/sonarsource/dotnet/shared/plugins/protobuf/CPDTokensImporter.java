/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins.protobuf;

import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toTextRange;

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.cpd.NewCpdTokens;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.CopyPasteTokenInfo;

class CPDTokensImporter extends ProtobufImporter<SonarAnalyzer.CopyPasteTokenInfo> {

  private final SensorContext context;

  CPDTokensImporter(SensorContext context) {
    super(SonarAnalyzer.CopyPasteTokenInfo.parser(), context, SonarAnalyzer.CopyPasteTokenInfo::getFilePath);
    this.context = context;
  }

  @Override
  void consumeFor(InputFile inputFile, CopyPasteTokenInfo message) {
    NewCpdTokens cpdTokens = context.newCpdTokens().onFile(inputFile);

    for (SonarAnalyzer.CopyPasteTokenInfo.TokenInfo tokenInfo : message.getTokenInfoList()) {
      cpdTokens.addToken(toTextRange(inputFile, tokenInfo.getTextRange()), tokenInfo.getTokenValue());
    }
    cpdTokens.save();
  }

}
