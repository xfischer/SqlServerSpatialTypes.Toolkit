﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.SqlServer.Types;

namespace SqlServerSpatial.Toolkit.Viewers
{
	public interface IClipboardHandler
	{
		void SetClipboardText();
		void Initialize(IEnumerable<SqlGeometry> geometries);
	}

	/// <summary>
	/// Service used for "Copy SQL" feature.
	/// </summary>
	public class ClipboardHandler : IClipboardHandler
	{
		// Clipoard (copy sql feature)
		List<SqlGeometry> _currentGeometries;
		private readonly bool _ACTIVATE_CLIPBOARD = true;
		private StringBuilder _geomSqlSrcBuilder;
		private StringBuilder _geomSqlSrcBuilderSELECT;
		private int _geomSqlSourceCount;

		public void Initialize(IEnumerable<SqlGeometry> geometries)
		{
			_currentGeometries = geometries.ToList();
		}

		public void SetClipboardText()
		{
			ResetSQLSource();
			foreach (var g in _currentGeometries)
			{
				AppendGeometryToSQLSource(g, null);
			}
			string data = getSQLSourceText();
			if (data != null) Clipboard.SetText(data);
		}

		// Clipoard (copy sql feature)
		private void ResetSQLSource()
		{
			if (_ACTIVATE_CLIPBOARD == false) return;

			_geomSqlSrcBuilder = null;
			_geomSqlSrcBuilderSELECT = null;
			_geomSqlSourceCount = 0;
		}
		public void AppendGeometryToSQLSource(SqlGeometry geom, string label)
		{
			if (_ACTIVATE_CLIPBOARD == false) return;

			if (_geomSqlSrcBuilder == null)
			{
				ResetSQLSource();
				_geomSqlSrcBuilder = new StringBuilder();
				_geomSqlSrcBuilderSELECT = new StringBuilder();
			}
			else
			{
				_geomSqlSrcBuilder.AppendLine();
				_geomSqlSrcBuilderSELECT.AppendLine();
				_geomSqlSrcBuilderSELECT.Append("UNION ALL ");
			}


			_geomSqlSrcBuilder.AppendFormat("DECLARE @g{0} geometry = geometry::STGeomFromText('{1}',{2})", ++_geomSqlSourceCount, geom.ToString(), geom.STSrid.Value);

			// TODO: Prevent SQL injection with the label param
			//SqlCommand com = new SqlCommand(string.Format("SELECT @g{0} AS geom, @Label AS Label", _geomSqlSourceCount));
			//label = label ?? "Geom 'cool' " + _geomSqlSourceCount.ToString();
			//com.Parameters.AddWithValue("@Label", label);

			label = label ?? "Geometry " + _geomSqlSourceCount.ToString();
			_geomSqlSrcBuilderSELECT.AppendFormat("SELECT @g{0} AS geom, '{1}' AS Label", _geomSqlSourceCount, label.Replace("'", "''"));
		}
		private string getSQLSourceText()
		{

			if (_ACTIVATE_CLIPBOARD == false) return null;
			if (_geomSqlSrcBuilder != null)
			{
				_geomSqlSrcBuilder.AppendLine();
				_geomSqlSrcBuilder.AppendLine();

				_geomSqlSrcBuilderSELECT.AppendLine();
				return string.Concat(_geomSqlSrcBuilder.ToString(), _geomSqlSrcBuilderSELECT.ToString());
			}
			else return null;
		}


	}
}
